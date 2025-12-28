using Kruta.Protocol;
using Kruta.Protocol.Serilizations;
using Kruta.Server.Handlers.Interface;
using Kruta.Shared.Models;
using Kruta.Shared.Models.DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Server.Handlers
{
    public class LoginHandler : IPacketHandler
    {
        public void Handle(ClientObject client, EAPacket packet)
        {
            byte[] nameBytes = packet.GetValueRaw(3);
            string nickname = Encoding.UTF8.GetString(nameBytes);
            client.Username = nickname;

            // 1. Отвечаем самому новичку (Успех)
            var response = EAPacket.Create(1, 0);
            client.Send(response);

            // 2. РАССЫЛКА (BROADCAST): Сообщаем всем остальным, что зашел новый игрок
            var newUserPacket = EAPacket.Create(2, 1);
            newUserPacket.SetValueRaw(3, Encoding.UTF8.GetBytes(nickname));

            lock (client._gameServer2.Clients)
            {
                foreach (var otherClient in client._gameServer2.Clients)
                {
                    // Отправляем пакет ВСЕМ, кроме самого новичка (он и так получит список позже через запрос)
                    if (otherClient.Id != client.Id)
                    {
                        otherClient.Send(newUserPacket);
                    }
                }
            }

            Console.WriteLine($"[LOGIN] Игрок {nickname} вошел. Оповещение разослано.");
        }
    }
}
