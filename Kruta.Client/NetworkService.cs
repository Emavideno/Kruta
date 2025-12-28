using Kruta.Protocol;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Kruta.Client
{
    public class NetworkService
    {
        private Socket _socket;
        public event Action<EAPacket> PacketReceived;

        public async Task ConnectAsync(string host, int port)
        {
            if (_socket?.Connected == true) return;

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await _socket.ConnectAsync(host, port);

            // Запускаем чтение в отдельном потоке
            _ = Task.Run(ReceiveLoop);
        }

        private async Task ReceiveLoop()
        {
            byte[] buffer = new byte[1024];
            List<byte> accumulatedData = new();

            while (true)
            {
                try
                {
                    int received = await _socket.ReceiveAsync(buffer, SocketFlags.None);
                    if (received == 0) break;

                    accumulatedData.AddRange(buffer.Take(received));

                    // Простая проверка: пытаемся парсить, пока парсится
                    while (true)
                    {
                        var packet = EAPacket.Parse(accumulatedData.ToArray());
                        if (packet == null) break;

                        // Оповещаем подписчиков (ViewModel-и)
                        PacketReceived?.Invoke(packet);

                        // Тут нужно удаление обработанных байт из accumulatedData 
                        // (реализуй расчет длины как мы обсуждали ранее)
                        accumulatedData.Clear(); // Упрощенно для примера
                    }
                }
                catch { break; }
            }
        }

        public void SendPacket(EAPacket packet)
        {
            _socket?.Send(packet.Encrypt().ToPacket());
        }
    }
}
