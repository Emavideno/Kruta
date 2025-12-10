using System;
using System.Threading.Tasks;
using Kruta.Shared.XProtocol; // Для XPacket и XPacketConverter
using Kruta.Shared.XMessages;
using Kruta.Shared.XMessages.ClientMessages; // Для клиентских сообщений (AuthMessage, PlayCardMessage и т.д.)
using Kruta.Shared.XMessages.ServerMessages; // Для серверных сообщений (ErrorMessage)
using Kruta.Server.Networking; // Для ClientConnection
using Kruta.Server.Logic; // Для IGameSessionManager

namespace Kruta.Server.Logic
{
    // В ClientConnection мы используем IGameSessionManager, 
    // поэтому PacketHandler должен быть частью класса/системы, 
    // которая управляет сессиями (например, GameEngine).
    public class PacketHandler
    {

        // В реальной игре здесь должна быть ссылка на IGameSessionManager/GameEngine
        private readonly IGameSessionManager _sessionManager;

        // Для демонстрации: счетчик игроков
        private int _playerIdCounter = 1;

        public PacketHandler(IGameSessionManager sessionManager)
        {
            _sessionManager = sessionManager;
        }

        /// <summary>
        /// Обрабатывает входящий XPacket от клиента.
        /// </summary>
        public async Task HandlePacketAsync(ClientConnection connection, XPacket xpacket)
        {
            var packetType = XPacketTypeManager.GetTypeFromPacket(xpacket);

            // В XProtocol ClientConnection уже обрабатывает Handshake и Auth (аутентификацию).
            // Здесь мы ожидаем только игровые команды.
            Console.WriteLine($"[HANDLER] Получен пакет типа: {packetType} от клиента {connection.ClientId}");

            switch (packetType)
            {
                case XPacketType.Auth:
                    // Auth уже должен быть обработан в ClientConnection, 
                    // но если мы хотим централизовать логику регистрации, 
                    // мы можем вызвать ее здесь.
                    ProcessAuth(connection, xpacket);
                    break;

                case XPacketType.PlayCard:
                    ProcessPlayCard(connection, xpacket);
                    break;

                case XPacketType.BuyCard:
                    ProcessBuyCard(connection, xpacket);
                    break;

                case XPacketType.EndTurn:
                    ProcessEndTurn(connection, xpacket);
                    break;

                case XPacketType.Handshake:
                    // Handshake должен быть обработан в ClientConnection
                    Console.WriteLine("[WARNING] Пакет Handshake получен в Game Logic (пропущен).");
                    break;

                default:
                    Console.WriteLine($"[WARNING] Неизвестный или неожиданный тип пакета в логике: {packetType}");
                    // Отправляем ошибку клиенту (используем метод из ClientConnection)
                    connection.QueuePacketSend(CreateErrorPacket("Неизвестная команда."));
                    break;
            }

            // После любого действия (Play, Buy, EndTurn) 
            // должен вызываться метод для обновления состояния игры и рассылки GameState
            // _sessionManager.BroadcastGameState(); 
        }

        // === МЕТОДЫ ОБРАБОТКИ ИГРОВЫХ КОМАНД ===

        private void ProcessAuth(ClientConnection connection, XPacket xpacket)
        {
            // 1. Десериализация Value Types
            var authMsg = XPacketConverter.Deserialize<AuthMessage>(xpacket);

            // 2. Десериализация строки (ручная)
            authMsg.DeserializeString(xpacket);

            // В ClientConnection мы уже проверили версию протокола.

            // 3. Выполняем логику регистрации
            int newPlayerId = RegisterPlayer(authMsg.PlayerName);
            //connection.PlayerId = newPlayerId; // В ClientConnection уже присвоен ClientId

            Console.WriteLine($"[LOGIC] Игрок {authMsg.PlayerName} успешно зарегистрирован с ID: {newPlayerId}");

            // Тут можно отправить Welcome/GameStateUpdate
        }

        private void ProcessPlayCard(ClientConnection connection, XPacket xpacket)
        {
            var playCardMsg = XPacketConverter.Deserialize<PlayCardMessage>(xpacket);

            Console.WriteLine($"[LOGIC] Игрок {connection.ClientId} хочет сыграть карту {playCardMsg.CardId} на игрока {playCardMsg.TargetPlayerId}.");

            // _sessionManager.ProcessPlayCard(connection.ClientId, playCardMsg.CardId, playCardMsg.TargetPlayerId);
        }

        private void ProcessBuyCard(ClientConnection connection, XPacket xpacket)
        {
            var buyCardMsg = XPacketConverter.Deserialize<BuyCardMessage>(xpacket);

            // Обратите внимание: старое поле Source убрано, используем только CardIdToBuy
            Console.WriteLine($"[LOGIC] Игрок {connection.ClientId} хочет купить карту ID: {buyCardMsg.CardIdToBuy}.");

            // _sessionManager.ProcessBuyCard(connection.ClientId, buyCardMsg.CardIdToBuy);
        }

        private void ProcessEndTurn(ClientConnection connection, XPacket xpacket)
        {
            // EndTurnMessage не содержит полей, но его нужно десериализовать для проверки типа
            // var endTurnMsg = XPacketConverter.Deserialize<EndTurnMessage>(xpacket); 

            Console.WriteLine($"[LOGIC] Игрок {connection.ClientId} завершает ход.");

            // _sessionManager.ProcessEndTurn(connection.ClientId);
        }

        // === ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===

        private int RegisterPlayer(string name)
        {
            // В реальном коде тут создается объект Player и добавляется в GameState
            return _playerIdCounter++;
        }

        private XPacket CreateErrorPacket(string message, int errorCode = 500)
        {
            var errorMsg = new ErrorMessage { ErrorCode = errorCode, Message = message };

            var packet = XPacketConverter.Serialize(XPacketType.Error, errorMsg);
            errorMsg.SerializeString(packet);

            return packet;
        }
    }
}