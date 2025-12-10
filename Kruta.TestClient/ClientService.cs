using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using Kruta.Shared.XProtocol;
using Kruta.Shared.XMessages;
using Kruta.Shared.XMessages.ClientMessages;
using Kruta.Shared.XMessages.ServerMessages;

namespace Kruta.TestClient
{
    public class ClientService
    {
        private TcpClient _client;
        private NetworkStream _stream;

        // --- ПОЛЯ ДЛЯ XPROTOCOL ---
        private const int BufferSize = 8192;
        private byte[] _receiveBuffer = new byte[BufferSize];
        private MemoryStream _packetCollector = new MemoryStream(); // Накопитель входящих байтов
        // --- КОНЕЦ ПОЛЕЙ ---

        // Используем ConcurrentDictionary, чтобы безопасно хранить ID игрока и его Имя
        private ConcurrentDictionary<int, string> _localPlayers =
            new ConcurrentDictionary<int, string>();

        private readonly string _playerName;
        private bool _isConnected = false;
        private bool _handshakeCompleted = false;

        public ClientService(string playerName)
        {
            _playerName = playerName;
        }

        public async Task ConnectAndRun(string ip, int port)
        {
            try
            {
                _client = new TcpClient();
                Console.WriteLine($"Попытка подключения к серверу {ip}:{port}...");

                await _client.ConnectAsync(ip, port);

                _stream = _client.GetStream();
                _isConnected = true;
                Console.WriteLine($"Успешно подключено к {ip}:{port}");

                // Запускаем асинхронный цикл приема пакетов
                var receiveTask = Task.Run(ReceiveLoop);

                // Цикл ввода команд (для тестирования)
                await InputLoop();

                await receiveTask;
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
            finally
            {
                Disconnect();
            }
        }

        private async Task InputLoop()
        {
            Console.WriteLine("\n--- Введите команду для отправки ---");
            Console.WriteLine("P [CardId] [TargetId] - Play Card");
            Console.WriteLine("B [CardId] - Buy Card");
            Console.WriteLine("E - End Turn");
            Console.WriteLine("X - Выход\n");

            while (_isConnected)
            {
                var input = await Task.Run(() => Console.ReadLine());

                if (string.IsNullOrEmpty(input)) continue;

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                var command = parts[0].ToUpper();

                switch (command)
                {
                    case "P": // Play Card [CardId] [TargetId]
                        if (parts.Length >= 3 && int.TryParse(parts[1], out var cardId) && int.TryParse(parts[2], out var targetId))
                        {
                            SendPlayCard(cardId, targetId);
                        }
                        else
                        {
                            Console.WriteLine("Неверный формат. Используйте P [CardId] [TargetId]");
                        }
                        break;
                    case "B": // Buy Card [CardId]
                        if (parts.Length >= 2 && int.TryParse(parts[1], out var buyCardId))
                        {
                            SendBuyCard(buyCardId);
                        }
                        else
                        {
                            Console.WriteLine("Неверный формат. Используйте B [CardId]");
                        }
                        break;
                    case "E": // End Turn
                        SendEndTurn();
                        break;
                    case "X":
                        return;
                    default:
                        Console.WriteLine($"Неизвестная команда: {command}");
                        break;
                }
            }
        }

        private async Task ReceiveLoop()
        {
            try
            {
                while (_isConnected)
                {
                    int bytesRead = await _stream.ReadAsync(_receiveBuffer, 0, _receiveBuffer.Length);

                    if (bytesRead == 0) // Сервер отключился
                    {
                        break;
                    }
                    ProcessReceivedBytes(_receiveBuffer, bytesRead);
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Соединение с сервером прервано.");
            }
            catch (ObjectDisposedException)
            {
                // Нормальное отключение
            }
            finally
            {
                Disconnect();
            }
        }

        // === МЕХАНИЗМ СБОРА И РАЗБОРА БИНАРНЫХ ПАКЕТОВ ===

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
                if (fullData.Length - offset < 7) break;

                var currentData = fullData.Skip(offset).ToArray();

                if (!currentData.Take(3).SequenceEqual(HeaderA) && !currentData.Take(3).SequenceEqual(HeaderB))
                {
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

                if (endingIndex == -1) break;

                var packetLength = endingIndex + 1;
                var rawPacket = currentData.Take(packetLength).ToArray();

                var xpacket = XPacket.Parse(rawPacket);

                if (xpacket != null)
                {
                    ProcessPacket(xpacket);
                }
                else
                {
                    Console.WriteLine("Ошибка: Не удалось распарсить XPacket.");
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

        // === ОБРАБОТКА ВХОДЯЩИХ СООБЩЕНИЙ ===

        private void ProcessPacket(XPacket packet)
        {
            var packetType = XPacketTypeManager.GetTypeFromPacket(packet);

            switch (packetType)
            {
                case XPacketType.Handshake:
                    ProcessHandshake(packet);
                    break;
                case XPacketType.GameStateUpdate:
                    ProcessGameStateUpdate(packet);
                    break;
                case XPacketType.Error:
                    ProcessError(packet);
                    break;
                case XPacketType.GameOver:
                    ProcessGameOver(packet);
                    break;
                case XPacketType.PlayerConnected: // <-- НОВЫЙ CASE
                    ProcessPlayerConnected(packet);
                    break;
                case XPacketType.PlayerDisconnected:
                    ProcessPlayerDisconnected(packet);
                    break;
                default:
                    Console.WriteLine($"[Client] Получен неожиданный пакет типа {packetType}");
                    break;
            }
        }

        private void ProcessHandshake(XPacket packet)
        {
            var handshakeMsg = XPacketConverter.Deserialize<XPacketHandshake>(packet);

            Console.WriteLine($"[Server] Получен Handshake с MagicNumber: {handshakeMsg.MagicHandshakeNumber}");

            if (!_handshakeCompleted)
            {
                // Отправляем ответ: MagicNumber + 15
                handshakeMsg.MagicHandshakeNumber += 15;
                var responsePacket = XPacketConverter.Serialize(XPacketType.Handshake, handshakeMsg);
                SendPacket(responsePacket);

                _handshakeCompleted = true;

                // СРАЗУ ПОСЛЕ HANDSHAKE ОТПРАВЛЯЕМ AUTH
                SendAuthPacket();
            }
        }

        private void ProcessGameStateUpdate(XPacket packet)
        {
            // Десериализация Value Types (GameStateUpdateMessage)
            var stateMsg = XPacketConverter.Deserialize<GameStateUpdateMessage>(packet);

            // Поскольку GameState — это struct с [XField], она десериализуется как часть пакета
            // Если бы мы хотели получить ее отдельно:
            // var gameState = XPacketConverter.Deserialize<GameState>(packet); 

            // Для простоты вывода:
            var gameState = stateMsg.CurrentState;

            Console.WriteLine("--- ОБНОВЛЕНИЕ СОСТОЯНИЯ ИГРЫ ---");
            Console.WriteLine($"Ход №: {gameState.TurnNumber}");
            Console.WriteLine($"Активный игрок: {gameState.ActivePlayerId}");
            Console.WriteLine($"Игра запущена: {gameState.IsGameRunning}");
        }

        private void ProcessError(XPacket packet)
        {
            var errorMsg = XPacketConverter.Deserialize<ErrorMessage>(packet);
            errorMsg.DeserializeString(packet); // Ручная десериализация строки

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[ОШИБКА {errorMsg.ErrorCode}] {errorMsg.Message}");
            Console.ResetColor();
        }

        private void ProcessGameOver(XPacket packet)
        {
            var msg = XPacketConverter.Deserialize<GameOverMessage>(packet);
            msg.DeserializeString(packet);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=====================================");
            Console.WriteLine($"!!! ИГРА ОКОНЧЕНА !!! Победитель: {msg.WinnerPlayerId}");
            Console.WriteLine($"Сводка: {msg.FinalScoreSummary}");
            Console.WriteLine("=====================================");
            Console.ResetColor();
        }

        private void ProcessPlayerConnected(XPacket packet)
        {
            var msg = XPacketConverter.Deserialize<PlayerConnectedMessage>(packet);
            msg.DeserializeString(packet); // Ручная десериализация имени

            // Добавление игрока в локальный список
            if (_localPlayers.TryAdd(msg.PlayerId, msg.PlayerName))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("-------------------------------------");
                Console.WriteLine($"[GAME NOTIFY] Новый игрок подключен: {msg.PlayerName} (ID: {msg.PlayerId})");
                Console.WriteLine("-------------------------------------");
                Console.ResetColor();
            }
            else
            {
                // Это должно случиться только один раз при подключении к игре, иначе это ошибка.
                Console.WriteLine($"[Client] Игрок {msg.PlayerName} (ID: {msg.PlayerId}) уже присутствует в локальном списке.");
            }
        }

        private void ProcessPlayerDisconnected(XPacket packet)
        {
            // 1. Десериализация сообщения
            var msg = XPacketConverter.Deserialize<PlayerDisconnectedMessage>(packet);

            // 2. УДАЛЕНИЕ ИГРОКА ИЗ ЛОКАЛЬНОГО СПИСКА
            if (_localPlayers.TryRemove(msg.PlayerId, out var playerName))
            {
                Console.WriteLine($"[CLIENT] Игрок {playerName} (ID: {msg.PlayerId}) отключился от сервера и удален.");
            }
            else
            {
                Console.WriteLine($"[CLIENT] Ошибка: Попытка удалить неизвестного игрока с ID {msg.PlayerId}.");
            }
        }


        // === МЕТОДЫ ОТПРАВКИ СООБЩЕНИЙ КЛИЕНТА ===

        private void SendPacket(XPacket packet)
        {
            if (!_isConnected || _stream == null) return;

            try
            {
                var bytes = packet.ToPacket();
                _stream.Write(bytes, 0, bytes.Length);
                _stream.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка отправки пакета: {ex.Message}");
                Disconnect();
            }
        }

        private void SendAuthPacket()
        {
            var authMsg = new AuthMessage
            {
                ProtocolVersion = 1,
                PlayerName = _playerName
            };

            var packet = XPacketConverter.Serialize(XPacketType.Auth, authMsg);
            authMsg.SerializeString(packet); // Ручная сериализация строки

            SendPacket(packet);
            Console.WriteLine($"[Client] Отправлен пакет Auth. Имя: {_playerName}");
        }

        private void SendPlayCard(int cardId, int targetPlayerId)
        {
            var msg = new PlayCardMessage
            {
                CardId = cardId,
                TargetPlayerId = targetPlayerId
            };
            var packet = XPacketConverter.Serialize(XPacketType.PlayCard, msg);
            SendPacket(packet);
            Console.WriteLine($"[Client] Отправлен PlayCard (Card: {cardId}, Target: {targetPlayerId})");
        }

        private void SendBuyCard(int cardIdToBuy)
        {
            var msg = new BuyCardMessage
            {
                PlayerId = 0, // Не используется, но должно быть заполнено
                CardIdToBuy = cardIdToBuy
            };
            var packet = XPacketConverter.Serialize(XPacketType.BuyCard, msg);
            SendPacket(packet);
            Console.WriteLine($"[Client] Отправлен BuyCard (Card: {cardIdToBuy})");
        }

        private void SendEndTurn()
        {
            var msg = new EndTurnMessage
            {
                PlayerId = 0 // Не используется
            };
            var packet = XPacketConverter.Serialize(XPacketType.EndTurn, msg);
            SendPacket(packet);
            Console.WriteLine("[Client] Отправлен EndTurn");
        }

        private void Disconnect()
        {
            if (_isConnected)
            {
                _isConnected = false;
                _stream?.Dispose();
                _client?.Close();
                Console.WriteLine("Клиент отключен.");
            }
        }
    }
}