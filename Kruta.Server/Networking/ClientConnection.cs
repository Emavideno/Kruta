using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Kruta.Shared.XProtocol;
using Kruta.Shared.XMessages;
using Kruta.Shared.XMessages.ClientMessages;
using Kruta.Shared.XMessages.ServerMessages;


// Предполагаем, что Kruta.Server.Logic содержит интерфейсы для взаимодействия
using Kruta.Server.Logic;

namespace Kruta.Server.Networking
{
    public class ClientConnection
    {
        private readonly NetworkStream _stream;
        private readonly TcpClient _client;

        // Исправлено: Используем интерфейс/класс для взаимодействия с основной логикой сервера
        private readonly IGameSessionManager _sessionManager;

        // --- ПОЛЯ ДЛЯ XPROTOCOL ---
        private const int BufferSize = 8192;
        private byte[] _receiveBuffer = new byte[BufferSize];
        private MemoryStream _packetCollector = new MemoryStream();
        // --- КОНЕЦ НОВЫХ ПОЛЕЙ ---

        public int ClientId { get; private set; }
        private ConcurrentQueue<XPacket> _incomingQueue = new ConcurrentQueue<XPacket>();

        private bool _isAuthenticated = false;

        // Конструктор теперь принимает IGameSessionManager
        public ClientConnection(TcpClient client, int clientId, IGameSessionManager sessionManager)
        {
            _client = client;
            ClientId = clientId;
            _stream = client.GetStream();
            _sessionManager = sessionManager;

            Console.WriteLine($"[Client {ClientId}] Подключен.");
        }

        public async Task ProcessAsync()
        {
            try
            {
                // ОТПРАВКА ПАКЕТА РУКОПОЖАТИЯ (Handshake)
                var rand = new Random();
                int magicHandshakeNumber = rand.Next();

                QueuePacketSend(CreateHandshakePacket(magicHandshakeNumber));

                // ОЖИДАНИЕ АУТЕНТИФИКАЦИИ ИЛИ HANDSHAKE-ОТВЕТА
                while (_client.Connected && !_isAuthenticated)
                {
                    await ReadAndProcessBytes();
                    await Task.Delay(10);
                }

                // ОСНОВНОЙ ЦИКЛ ОБРАБОТКИ
                while (_client.Connected && _isAuthenticated)
                {
                    await ReadAndProcessBytes();
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[Client {ClientId}] Отключен. Ошибка: {ex.Message}");
            }
            catch (ObjectDisposedException)
            {
                Console.WriteLine($"[Client {ClientId}] Соединение закрыто.");
            }
            finally
            {
                // Используем IGameSessionManager для удаления клиента
                _sessionManager.RemoveClient(ClientId);
                _client.Close();
            }
        }

        private async Task ReadAndProcessBytes()
        {
            int bytesRead = await _stream.ReadAsync(_receiveBuffer, 0, _receiveBuffer.Length);

            if (bytesRead == 0) // Клиент отключился
            {
                throw new IOException("Соединение закрыто клиентом.");
            }

            // Передача прочитанных данных в механизм сбора пакетов
            ProcessReceivedBytes(_receiveBuffer, bytesRead);

            // Обработка всех собранных пакетов
            if (!_incomingQueue.IsEmpty)
            {
                ProcessIncomingPackets();
            }
        }

        // === МЕХАНИЗМ СБОРА И РАЗБОРА БИНАРНЫХ ПАКЕТОВ (ИЗ СТАТЬИ) ===

        private void ProcessReceivedBytes(byte[] buffer, int bytesCount)
        {
            _packetCollector.Write(buffer, 0, bytesCount);
            ExtractPackets();
        }

        private void ExtractPackets()
        {
            var fullData = _packetCollector.ToArray();
            var offset = 0;

            var HeaderA = new byte[] { 0xAF, 0xAA, 0xAF };
            var HeaderB = new byte[] { 0x95, 0xAA, 0xFF };
            var Ending = new byte[] { 0xFF, 0x00 };

            while (true)
            {
                if (fullData.Length - offset < 7)
                {
                    break;
                }

                var currentData = fullData.Skip(offset).ToArray();

                if (!currentData.Take(3).SequenceEqual(HeaderA) && !currentData.Take(3).SequenceEqual(HeaderB))
                {
                    // Предполагаем, что это мусор или ошибка, сдвигаемся на 1 байт и ищем заново
                    offset++;
                    continue;
                }

                var endingIndex = -1;
                for (int i = 3; i < currentData.Length - 1; i++)
                {
                    if (currentData[i] == Ending[0] && currentData[i + 1] == Ending[1])
                    {
                        endingIndex = i + 1;
                        break;
                    }
                }

                if (endingIndex == -1)
                {
                    break;
                }

                var packetLength = endingIndex + 1;
                var rawPacket = currentData.Take(packetLength).ToArray();

                var xpacket = XPacket.Parse(rawPacket);
                if (xpacket != null)
                {
                    _incomingQueue.Enqueue(xpacket);
                }
                else
                {
                    Console.WriteLine($"[Client {ClientId}] Ошибка парсинга XPacket. Длина: {rawPacket.Length}");
                }

                offset += packetLength;
            }

            if (offset > 0)
            {
                var remainingData = fullData.Skip(offset).ToArray();
                _packetCollector.SetLength(0);
                _packetCollector.Write(remainingData, 0, remainingData.Length);
            }
        }

        // === ОТПРАВКА ПАКЕТОВ ===

        public void QueuePacketSend(XPacket packet)
        {
            try
            {
                var bytes = packet.ToPacket();
                _stream.Write(bytes, 0, bytes.Length);
                // Отправка в потоке синхронна, но для UDP или более сложных систем здесь была бы очередь.
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[Client {ClientId}] Ошибка отправки: {ex.Message}");
            }
            catch (ObjectDisposedException)
            {
                // Клиент отключился
            }
        }

        // Вспомогательный метод для создания пакета Handshake
        private XPacket CreateHandshakePacket(int magicNumber)
        {
            var handshakeMsg = new XPacketHandshake { MagicHandshakeNumber = magicNumber };
            return XPacketConverter.Serialize(XPacketType.Handshake, handshakeMsg);
        }

        // Вспомогательный метод для создания пакета Error
        private XPacket CreateErrorPacket(string message, int errorCode = 500)
        {
            var errorMsg = new ErrorMessage { ErrorCode = errorCode, Message = message };

            var packet = XPacketConverter.Serialize(XPacketType.Error, errorMsg);
            errorMsg.SerializeString(packet); // Ручная сериализация строки

            return packet;
        }

        // === ОБРАБОТКА ВХОДЯЩИХ ПАКЕТОВ ===

        private void ProcessIncomingPackets()
        {
            while (_incomingQueue.TryDequeue(out var packet))
            {
                var packetType = XPacketTypeManager.GetTypeFromPacket(packet);

                switch (packetType)
                {
                    case XPacketType.Handshake:
                        ProcessHandshake(packet);
                        break;
                    case XPacketType.Auth:
                        ProcessAuth(packet);
                        break;
                    case XPacketType.PlayCard:
                        ProcessPlayCard(packet);
                        break;
                    case XPacketType.BuyCard:
                        ProcessBuyCard(packet);
                        break;
                    case XPacketType.EndTurn:
                        ProcessEndTurn(packet);
                        break;
                    case XPacketType.Unknown:
                        Console.WriteLine($"[Client {ClientId}] Получен неизвестный пакет.");
                        QueuePacketSend(CreateErrorPacket("Неизвестный тип пакета."));
                        break;
                    default:
                        Console.WriteLine($"[Client {ClientId}] Получен неожиданный пакет типа {packetType}");
                        break;
                }
            }
        }

        // === РЕАЛИЗАЦИЯ МЕТОДОВ ОБРАБОТКИ (Заглушки) ===

        private void ProcessHandshake(XPacket packet)
        {
            Console.WriteLine($"[Client {ClientId}] Получен ответ Handshake.");

            var handshakeMsg = XPacketConverter.Deserialize<XPacketHandshake>(packet);

            // Логика из статьи: отвечаем числом на 15 меньше
            handshakeMsg.MagicHandshakeNumber -= 15;

            // Отправляем обратно, потенциально зашифрованным
            var responsePacket = XPacketConverter.Serialize(XPacketType.Handshake, handshakeMsg);

            // Здесь можно решить, шифровать ли ответ
            // QueuePacketSend(responsePacket.Encrypt()); 

            QueuePacketSend(responsePacket);

            // Если рукопожатие успешно, можно разрешить аутентификацию
            // (В этой версии, аутентификация разрешена по умолчанию после Handshake)
        }

        private void ProcessAuth(XPacket packet)
        {
            if (_isAuthenticated)
            {
                QueuePacketSend(CreateErrorPacket("Вы уже аутентифицированы."));
                return;
            }

            // 1. Десериализация Value Types
            var authMsg = XPacketConverter.Deserialize<AuthMessage>(packet);

            // 2. Десериализация строки (ручная)
            authMsg.DeserializeString(packet);

            if (authMsg.ProtocolVersion != 1)
            {
                QueuePacketSend(CreateErrorPacket($"Несовместимая версия протокола: {authMsg.ProtocolVersion}.", 400));
                return;
            }

            // <--- ИЗМЕНЕНИЕ ЗДЕСЬ: ПЕРЕДАЕМ УПРАВЛЕНИЕ МЕНЕДЖЕРУ --->

            // Регистрируем нового игрока, что вызовет рассылку PlayerConnected всем
            _sessionManager.RegisterNewPlayer(ClientId, authMsg.PlayerName);

            _isAuthenticated = true;
            Console.WriteLine($"[Client {ClientId}] Аутентифицирован как: {authMsg.PlayerName}");
        }

        private void ProcessPlayCard(XPacket packet)
        {
            var msg = XPacketConverter.Deserialize<PlayCardMessage>(packet);

            Console.WriteLine($"[Client {ClientId}] Разыгрывает карту ID: {msg.CardId} на игрока {msg.TargetPlayerId}. (ЗАГЛУШКА)");

            // Логика: Проверить ход, проверить карту, обновить состояние игры, разослать GameStateUpdate всем.
        }

        private void ProcessBuyCard(XPacket packet)
        {
            var msg = XPacketConverter.Deserialize<BuyCardMessage>(packet);

            Console.WriteLine($"[Client {ClientId}] Покупает карту (ID {msg.CardIdToBuy}). (ЗАГЛУШКА)");

            // Логика: Выдать карту, обновить GameState.
        }

        private void ProcessEndTurn(XPacket packet)
        {
            var msg = XPacketConverter.Deserialize<EndTurnMessage>(packet);

            Console.WriteLine($"[Client {ClientId}] Завершает ход. (ЗАГЛУШКА)");

            // Логика: Сменить активного игрока, проверить условия победы, отправить GameStateUpdate.
        }

        /// <summary>
        /// Безопасно закрывает сетевое соединение с клиентом.
        /// </summary>
        public void Close()
        {
            // Устанавливаем флаг, чтобы остановить циклы ProcessAsync и ReceiveLoop
            // Хотя в текущем коде ProcessAsync управляется флагом _client.Connected, 
            // это полезно для явной индикации.
            // if (_client.Connected) // Можно добавить для проверки
            // {
            _stream?.Dispose();
            _client?.Close();
            // }
            Console.WriteLine($"[Client {ClientId}] Принудительное закрытие соединения.");
        }
    }
}