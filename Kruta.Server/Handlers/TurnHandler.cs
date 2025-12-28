using Kruta.Protocol;
using Kruta.Server.Handlers.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Server.Handlers
{
    public class TurnHandler : IPacketHandler
    {
        public void Handle(ClientObject client, EAPacket packet)
        {
            // Subtype 0: Игрок хочет завершить ход
            if (packet.PacketSubtype == 0)
            {
                var server = client._gameServer2;

                lock (server.Clients)
                {
                    // 1. ПРОВЕРКА: А действительно ли это ход этого игрока?
                    // (Защита от читеров или лагов, чтобы нельзя было скипнуть чужой ход)
                    if (server.Clients.Count > server.CurrentPlayerIndex)
                    {
                        var expectedPlayer = server.Clients[server.CurrentPlayerIndex];

                        if (expectedPlayer.Id == client.Id)
                        {
                            Console.WriteLine($"[GAME] Игрок {client.Username} завершил ход.");

                            // 2. Передаем ход следующему
                            server.NextTurn();
                        }
                        else
                        {
                            Console.WriteLine($"[WARN] Игрок {client.Username} попытался завершить ход не в свою очередь! (Сейчас ходит: {expectedPlayer.Username})");
                        }
                    }
                }
            }
        }
    }
}
