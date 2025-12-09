using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Kruta.Server.Networking
{
    public class GameServer
    {
        private TcpListener _listener;
        private readonly int _port = 13000;
        private bool _isRunning = true;

        public GameServer()
        {
            // Используем IPAddress.Any для прослушивания на всех сетевых интерфейсах
            _listener = new TcpListener(IPAddress.Any, _port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine($"[SERVER] Запущен и прослушивает порт {_port}...");

            try
            {
                while (_isRunning)
                {
                    // Асинхронно ждем нового клиента
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine($"[SERVER] Принят новый клиент: {client.Client.RemoteEndPoint}");

                    // Запускаем обработку соединения в отдельной задаче
                    _ = HandleClientConnection(client);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"[ERROR] Ошибка сокета в цикле Accept: {ex.Message}");
            }
            finally
            {
                _listener.Stop();
            }
        }

        private async Task HandleClientConnection(TcpClient client)
        {
            // Создаем экземпляр ClientConnection для обработки этого клиента
            var connection = new ClientConnection(client);
            await connection.ProcessAsync();
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
        }
    }
}
