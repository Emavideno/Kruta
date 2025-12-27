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
            Debug.WriteLine("[NET] Цикл чтения запущен, жду байты...");
            try
            {
                while (!token.IsCancellationRequested && _client.Connected)
                {
                    int b = _stream.ReadByte(); //мы читаем по одному байту
                    if (b == -1)
                    {
                        Debug.WriteLine("[NET] Сервер разорвал соединение.");
                        break;
                    }

                    buffer.Add((byte)b); //закидываем этот 1 байт в буффер

                    
                    //если увидели что в буффере >6 (min размер пакета нашего с протоколом)
                    //и концовка AE, то значит тут уже лежит целый пакет и можно начинать парсить
                    if (buffer.Count >= 6 && buffer[^2] == 0x41 && buffer[^1] == 0x45) 
                    {
                        Debug.WriteLine($"[NET] Пойман полный пакет! Длина: {buffer.Count}. Парсим...");
                        var raw = buffer.ToArray(); //из листа делаем массив байтов, потому что в методе Parse принимается именно массив
                        var packet = EAPacket.Parse(raw);

                        if (packet != null)
                        {
                            Debug.WriteLine($"[NET] Пакет успешно распаршен! Тип: {packet.PacketType}, Подтип: {packet.PacketSubtype}");
                            MainThread.BeginInvokeOnMainThread(() => {
                                OnPacketReceived?.Invoke(packet);
                            });
                        }
                        else
                        {
                            Debug.WriteLine("[NET] ПРОВАЛ: Parse вернул null. Проверь заголовки (EA/CS)!");
                        }
                        buffer.Clear();
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"[NET] Ошибка в цикле: {ex.Message}"); }
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