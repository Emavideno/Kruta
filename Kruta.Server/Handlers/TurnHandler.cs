using Kruta.Protocol;
using Kruta.Server.Handlers.Interface;
using System;
using System.Text;

namespace Kruta.Server.Handlers
{
    public class TurnHandler : IPacketHandler
    {
        public void Handle(ClientObject client, EAPacket packet)
        {
            var server = client._gameServer2;

            lock (server.Clients)
            {
                if (server.Clients.Count == 0 || server.CurrentPlayerIndex >= server.Clients.Count)
                    return;

                var expectedPlayer = server.Clients[server.CurrentPlayerIndex];

                // Проверка очереди хода
                if (expectedPlayer.Id != client.Id)
                {
                    Console.WriteLine($"[WARN] Игрок {client.Username} пытался действовать не в свой ход!");
                    return;
                }

                switch (packet.PacketSubtype)
                {
                    case 0: // Конец хода
                        Console.WriteLine($"[GAME] Игрок {client.Username} завершил ход.");
                        server.NextTurn();
                        break;

                    case 2: // Атака
                        if (packet.HasField(3))
                        {
                            string targetName = Encoding.UTF8.GetString(packet.GetValueRaw(3)).Trim();
                            server.ProcessAttack(client, targetName);
                        }
                        break;

                    case 3: // Розыгрыш карты
                        if (packet.HasField(3))
                        {
                            int cardId = BitConverter.ToInt32(packet.GetValueRaw(3), 0);
                            server.ProcessPlayCard(client, cardId);
                        }
                        break;

                    case 4: // КУПИТЬ КАРТУ (Новое)
                        if (packet.HasField(3))
                        {
                            int cardId = BitConverter.ToInt32(packet.GetValueRaw(3), 0);
                            server.ProcessBuyCard(client, cardId);
                        }
                        break;

                    default:
                        Console.WriteLine($"[WARN] Неизвестный подтип хода: {packet.PacketSubtype}");
                        break;
                }
            }
        }
    }
}