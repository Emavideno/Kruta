using System;
using System.Collections.Generic;
using Kruta.Shared.Network.Enums;
using Kruta.Shared.Network.Protocol;
using Kruta.Shared.Network.Messages.ClientMessages; // Для десериализации
using Kruta.Server.Networking; // Для ClientConnection
using System.Text.Json; // Для десериализации

namespace Kruta.Server.Logic
{
    public class PacketHandler
    {
        // В реальной игре здесь должна быть ссылка на GameEngine
        // private readonly GameEngine _gameEngine = new GameEngine(); 

        public async Task HandlePacketAsync(ClientConnection connection, Packet packet)
        {
            Console.WriteLine($"[HANDLER] Получен пакет типа: {packet.Type} от клиента {connection.PlayerId}");

            // Используем switch по типу сообщения для десериализации в нужный класс
            switch (packet.Type)
            {
                case MessageType.Auth:
                    // 1. Десериализуем AuthMessage
                    var authMessage = packet.Deserialize<AuthMessage>();

                    // 2. Выполняем логику аутентификации/регистрации
                    int newPlayerId = RegisterPlayer(authMessage.PlayerName);
                    connection.PlayerId = newPlayerId;

                    // 3. Отправляем ответ Welcome
                    // await connection.SendMessageAsync(new WelcomeMessage { PlayerId = newPlayerId }); // Если нужно WelcomeMessage

                    break;

                case MessageType.PlayCard:
                    var playCardMsg = packet.Deserialize<PlayCardMessage>();
                    // _gameEngine.ProcessPlayCard(connection.PlayerId, playCardMsg.CardId, playCardMsg.TargetPlayerId);

                    // Заглушка:
                    Console.WriteLine($"Игрок {connection.PlayerId} хочет сыграть карту {playCardMsg.CardId}");

                    break;

                case MessageType.BuyCard:
                    var buyCardMsg = packet.Deserialize<BuyCardMessage>();
                    // _gameEngine.ProcessBuyCard(connection.PlayerId, buyCardMsg.CardId, buyCardMsg.Source);

                    // Заглушка:
                    Console.WriteLine($"Игрок {connection.PlayerId} хочет купить {buyCardMsg.CardId} из {buyCardMsg.Source}");

                    break;

                // ... (Обработка всех остальных MessageType) ...

                case MessageType.EndTurn:
                    // _gameEngine.ProcessEndTurn(connection.PlayerId);
                    Console.WriteLine($"Игрок {connection.PlayerId} завершает ход.");
                    break;

                default:
                    Console.WriteLine($"[WARNING] Неизвестный тип пакета: {packet.Type}");
                    break;
            }

            // После любого действия (Play, Buy, EndTurn) сервер должен отправить всем клиентам
            // новое состояние игры:
            // await BroadcastGameState(currentGameState);
        }

        private int _playerIdCounter = 1;
        private int RegisterPlayer(string name)
        {
            Console.WriteLine($"[LOGIC] Регистрируется новый игрок: {name}");
            // В реальном коде тут создается объект Player и добавляется в GameState
            return _playerIdCounter++;
        }
    }
}
