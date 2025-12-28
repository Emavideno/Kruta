using System.Net.Sockets;
using System.Diagnostics;
using Kruta.Protocol;

namespace Kruta.GUI2.Services
{
    public class NetworkService
    {
        public string PlayerName { get; set; }

        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _cts;

        public event Action<EAPacket> OnPacketReceived;

        public async Task<bool> ConnectAsync(string host, int port)
        {
            try
            {
                Disconnect(); // На всякий случай чистим старое
                _client = new TcpClient();
                await _client.ConnectAsync(host, port);
                _stream = _client.GetStream();
                _cts = new CancellationTokenSource();

                // Запускаем чтение в фоне
                _ = Task.Run(() => ReceiveLoop(_cts.Token));
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NET] Ошибка подключения: {ex.Message}");
                return false;
            }
        }

        private async Task ReceiveLoop(CancellationToken token)
        {
            var buffer = new List<byte>();
            Debug.WriteLine("[NET] Цикл чтения запущен...");

            try
            {
                while (!token.IsCancellationRequested && _client.Connected)
                {
                    // Читаем доступные байты
                    byte[] temp = new byte[1024];
                    int received = await _stream.ReadAsync(temp, 0, temp.Length, token);

                    if (received == 0) break; // Сервер закрыл соединение

                    for (int i = 0; i < received; i++)
                        buffer.Add(temp[i]);

                    // Пытаемся вытащить из буфера столько пакетов, сколько там есть
                    bool foundPacket;
                    do
                    {
                        foundPacket = false;
                        if (buffer.Count < 6) break;

                        // Ищем маркер начала (EA = 0x45, 0x41 или CS = 0x43, 0x53)
                        // Ищем маркер конца (AE = 0x41, 0x45)
                        int endIdx = -1;
                        for (int i = 0; i < buffer.Count - 1; i++)
                        {
                            if (buffer[i] == 0x41 && buffer[i + 1] == 0x45)
                            {
                                endIdx = i + 1;
                                break;
                            }
                        }

                        if (endIdx != -1)
                        {
                            // Вырезаем пакет
                            var packetData = buffer.Take(endIdx + 1).ToArray();
                            var packet = EAPacket.Parse(packetData);

                            if (packet != null)
                            {
                                MainThread.BeginInvokeOnMainThread(() => {
                                    OnPacketReceived?.Invoke(packet);
                                });
                            }

                            // УДАЛЯЕМ только обработанные байты, остальное оставляем!
                            buffer.RemoveRange(0, endIdx + 1);
                            foundPacket = true; // Проверим, вдруг там есть еще один пакет
                        }
                    } while (foundPacket);
                }
            }
            catch (Exception ex) { Debug.WriteLine($"[NET] Ошибка: {ex.Message}"); }
        }

        public void SendPacket(EAPacket packet)
        {
            if (_client is { Connected: true } && _stream != null)
            {
                try
                {
                    byte[] data = packet.ToPacket();
                    _stream.Write(data, 0, data.Length);
                }
                catch (Exception ex) { Debug.WriteLine(ex.Message); }
            }
        }

        public void Disconnect()
        {
            _cts?.Cancel();
            _stream?.Dispose();
            _client?.Close();
        }
    }
}