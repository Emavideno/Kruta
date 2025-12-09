using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.Network.Protocol;
using System.Net.Sockets;
using System.Text.Json;
using Kruta.Server.Logic;

namespace Kruta.Server.Networking
{
    public class ClientConnection
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly PacketHandler _handler = new PacketHandler();

        // Буфер для приема данных (можно регулировать размер)
        private const int BufferSize = 4096;
        private byte[] _buffer = new byte[BufferSize];

        public int PlayerId { get; internal set; } // ID игрока после успешной Auth

        public ClientConnection(TcpClient client)
        {
            _client = client;
            _stream = client.GetStream();
        }

        public async Task ProcessAsync()
        {
            try
            {
                // Для простоты, мы будем использовать простой разделитель \n 
                // между JSON-объектами, чтобы разделить Пакеты в потоке.

                var messageBuilder = new StringBuilder();

                while (_client.Connected)
                {
                    // Читаем данные из потока
                    int bytesRead = await _stream.ReadAsync(_buffer, 0, _buffer.Length);

                    if (bytesRead == 0) // Клиент отключился
                    {
                        Console.WriteLine($"[CONNECTION] Клиент {_client.Client.RemoteEndPoint} отключился.");
                        break;
                    }

                    // Добавляем прочитанные данные в буфер сообщений
                    messageBuilder.Append(Encoding.UTF8.GetString(_buffer, 0, bytesRead));

                    // Обрабатываем сообщения, разделенные символом \n
                    // Это критично для TCP, так как сообщения могут приходить склеенными (Streaming)

                    string fullMessage = messageBuilder.ToString();

                    while (true)
                    {
                        int delimiterIndex = fullMessage.IndexOf('\n');
                        if (delimiterIndex == -1)
                        {
                            // Нет полного сообщения, ждем следующего чтения
                            messageBuilder.Clear();
                            messageBuilder.Append(fullMessage);
                            break;
                        }

                        // Получено полное сообщение
                        string packetJson = fullMessage.Substring(0, delimiterIndex);
                        fullMessage = fullMessage.Substring(delimiterIndex + 1); // Остаток идет на следующую итерацию

                        // 1. Десериализация в Packet (Конверт)
                        Packet packet = JsonSerializer.Deserialize<Packet>(packetJson);

                        // 2. Обработка Пакета
                        if (packet != null)
                        {
                            // Передаем пакет центральному обработчику
                            await _handler.HandlePacketAsync(this, packet);
                        }
                    }
                }
            }
            catch (IOException)
            {
                // Ошибка чтения/записи, например, при внезапном разрыве соединения
                Console.WriteLine($"[ERROR] Соединение с клиентом {PlayerId} прервано.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[FATAL] Необработанная ошибка обработки клиента: {ex.Message}");
            }
            finally
            {
                _client.Close();
                // Тут должна быть логика очистки (удаление игрока из GameState)
            }
        }

        // Метод для отправки любого сообщения клиенту
        public async Task SendMessageAsync<T>(T message) where T : IMessage
        {
            // 1. Упаковываем сообщение в Packet
            var packet = Packet.Create(message);

            // 2. Сериализуем Packet в JSON и добавляем разделитель \n
            string jsonString = JsonSerializer.Serialize(packet) + "\n";
            byte[] buffer = Encoding.UTF8.GetBytes(jsonString);

            // 3. Отправляем в поток
            await _stream.WriteAsync(buffer, 0, buffer.Length);
        }
    }
}
