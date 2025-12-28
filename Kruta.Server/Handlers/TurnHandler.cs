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
            var server = client._gameServer2;

            // Проверяем подтип: 0 - это "Конец хода"
            if (packet.PacketSubtype == 0)
            {
                lock (server.Clients)
                {
                    // Проверка: тот ли игрок прислал пакет, чей сейчас ход?
                    // (Используем метод GetCurrentPlayerIndex, который в GameServer2)
                    if (server.Clients.IndexOf(client) == server.CurrentPlayerIndex)
                    {
                        Console.WriteLine($"[TURN] Игрок {client.Username} закончил ход.");
                        server.NextTurn();
                    }
                    else
                    {
                        Console.WriteLine($"[WARN] Игрок {client.Username} пытался закончить чужой ход!");
                    }
                }
            }
        }
    }
}
