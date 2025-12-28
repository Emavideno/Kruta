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

                // Проверка: только текущий игрок может совершать действия типа 5
                if (expectedPlayer.Id != client.Id)
                {
                    Console.WriteLine($"[WARN] Игрок {client.Username} пытался совершить действие не в свой ход!");
                    return;
                }

                // Subtype 0: Завершение хода
                if (packet.PacketSubtype == 0)
                {
                    Console.WriteLine($"[GAME] Игрок {client.Username} завершил ход.");
                    server.NextTurn();
                }

                // Subtype 2: Атака
                else if (packet.PacketSubtype == 2)
                {
                    if (packet.HasField(3))
                    {
                        string targetName = Encoding.UTF8.GetString(packet.GetValueRaw(3)).Trim();
                        server.ProcessAttack(client, targetName);
                    }
                }

                // Subtype 3: Розыгрыш карты
                else if (packet.PacketSubtype == 3)
                {
                    if (packet.HasField(3))
                    {
                        int cardId = BitConverter.ToInt32(packet.GetValueRaw(3), 0);
                        server.ProcessPlayCard(client, cardId);
                    }
                }
                else
                {
                    Console.WriteLine($"[WARN] Неизвестный подтип хода: {packet.PacketSubtype}");
                }
            }
        }
    }
}