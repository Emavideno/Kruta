using Kruta.Shared.Network.Messages.ClientMessages;
using Kruta.Shared.Network.Messages.ServerMessages;
using Kruta.Shared.Network.Protocol;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace Kruta.TestClient
{
    public class ClientService
    {
        private TcpClient _client;
        private NetworkStream _stream;

        private const string IpAddress = "127.0.0.1"; // Локальный адрес
        private const int Port = 13000;

        public async Task ConnectAsync()
        {
            _client = new TcpClient();
            try
            {
                await _client.ConnectAsync(IpAddress, Port);
                _stream = _client.GetStream();
                Console.WriteLine("[CLIENT] Успешно подключен к серверу.");

                // Запуск асинхронного приема данных от сервера
                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT ERROR] Не удалось подключиться: {ex.Message}");
            }
        }

        // Метод для отправки любого IMessage
        public async Task SendMessageAsync<T>(T message) where T : IMessage
        {
            if (_client == null || !_client.Connected)
            {
                Console.WriteLine("[CLIENT] Не подключен к серверу.");
                return;
            }

            // 1. Упаковка сообщения в Packet
            var packet = Packet.Create(message);

            // 2. Сериализация Packet в JSON и добавление РАЗДЕЛИТЕЛЯ \n
            string jsonString = JsonSerializer.Serialize(packet) + "\n";
            byte[] buffer = Encoding.UTF8.GetBytes(jsonString);

            // 3. Отправка
            await _stream.WriteAsync(buffer, 0, buffer.Length);
            Console.WriteLine($"[CLIENT] Отправлен пакет типа: {message.Type}");
        }

        // Асинхронный метод для приема данных от сервера
        private async Task ReceiveMessagesAsync()
        {
            var buffer = new byte[4096];
            var messageBuilder = new StringBuilder();

            try
            {
                while (_client.Connected)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    messageBuilder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));

                    string fullMessage = messageBuilder.ToString();

                    while (true)
                    {
                        int delimiterIndex = fullMessage.IndexOf('\n');
                        if (delimiterIndex == -1)
                        {
                            messageBuilder.Clear();
                            messageBuilder.Append(fullMessage);
                            break;
                        }

                        string packetJson = fullMessage.Substring(0, delimiterIndex);
                        fullMessage = fullMessage.Substring(delimiterIndex + 1);

                        // Попытка десериализации полученного Пакета
                        try
                        {
                            var packet = JsonSerializer.Deserialize<Packet>(packetJson);
                            Console.WriteLine($"[SERVER RECV] Тип: {packet.Type} | Payload: {packet.Payload}");

                            // Здесь должна быть логика обработки сообщений типа GameStateUpdate, Error и т.д.
                        }
                        catch (JsonException)
                        {
                            Console.WriteLine("[SERVER RECV ERROR] Некорректный JSON.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT RECV ERROR] Поток чтения прерван: {ex.Message}");
            }
        }
    }
}