using Kruta.Protocol;
using Kruta.Server.Handlers.Interface;
using Kruta.Shared.Mini;
using Kruta.Shared.Mini.Cards;
using System;
using System.Linq;
using System.Text;

namespace Kruta.Server.Handlers
{
    public class PlayCardHandler : IPacketHandler
    {
        public void Handle(ClientObject client, EAPacket packet)
        {
            if (!packet.HasField(3)) return;

            int cardId = BitConverter.ToInt32(packet.GetValueRaw(3), 0);

            Console.WriteLine($"[GAME] Игрок {client.Username} сыграл карту ID={cardId}");

            // Здесь создаём объект карты на сервере (для логики)
            ICardMini card = CardFactoryMini.CreateCardById(cardId);

            // Обновляем HP, Power или другие эффекты
            client.PlayerData.Hp += card.HealthBonus;
            client.PlayerData.Power += card.PowerBonus;

            // Рассылка всем игрокам, чтобы они видели, что этот игрок сыграл карту
            var broadcast = EAPacket.Create(6, 2); // Subtype 2 — информируем остальных
            broadcast.SetValueRaw(3, Encoding.UTF8.GetBytes(client.Username));
            broadcast.SetValueRaw(4, BitConverter.GetBytes(cardId));

            lock (client._gameServer2.Clients)
            {
                foreach (var c in client._gameServer2.Clients)
                {
                    if (c.Id != client.Id) c.Send(broadcast);
                }
            }
        }
    }
}
