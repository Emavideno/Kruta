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
            // Получаем ссылку на сервер через объект клиента
            var server = client._gameServer2;

            // Блокируем список клиентов для потокобезопасности
            lock (server.Clients)
            {
                // Проверка: есть ли вообще игроки и корректен ли индекс текущего игрока
                if (server.Clients.Count == 0 || server.CurrentPlayerIndex >= server.Clients.Count)
                    return;

                var expectedPlayer = server.Clients[server.CurrentPlayerIndex];

                // ПРОВЕРКА ОЧЕРЕДИ: только текущий игрок может совершать действия типа 5 (TurnAction)
                if (expectedPlayer.Id != client.Id)
                {
                    Console.WriteLine($"[WARN] Игрок {client.Username} пытался совершить действие (Subtype: {packet.PacketSubtype}) не в свой ход!");
                    return;
                }

                // --- ОБРАБОТКА ПОДТИПОВ ---

                // Subtype 0: Завершение хода
                if (packet.PacketSubtype == 0)
                {
                    Console.WriteLine($"[GAME] Игрок {client.Username} завершил ход.");
                    server.NextTurn();
                }

                // Subtype 2: Атака на другого игрока
                else if (packet.PacketSubtype == 2)
                {
                    if (packet.HasField(3))
                    {
                        byte[] rawData = packet.GetValueRaw(3);
                        if (rawData != null)
                        {
                            string targetName = Encoding.UTF8.GetString(rawData).Trim();

                            Console.WriteLine($"[GAME] Игрок {client.Username} инициировал атаку на {targetName}.");

                            // Вызываем логику расчёта урона и рассылки HP в GameServer2
                            server.ProcessAttack(client, targetName);
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[ERROR] Получен пакет атаки от {client.Username}, но поле с именем цели (3) отсутствует.");
                    }
                }
                else
                {
                    Console.WriteLine($"[WARN] Получен неизвестный подтип хода ({packet.PacketSubtype}) от {client.Username}.");
                }
            }
        }
    }
}