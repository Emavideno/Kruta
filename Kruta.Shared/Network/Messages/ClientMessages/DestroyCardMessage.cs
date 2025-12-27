using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.Network.Enums;
using Kruta.Shared.Network.Protocol;

namespace Kruta.Shared.Network.Messages.ClientMessages
{
    public class DestroyCardMessage : IMessage
    {
        public MessageType Type => MessageType.DestroyCard;
        public int CardId { get; set; } // Карта игрока, которую нужно удалить
        public int PlayerId { get; set; } // Чью карту уничтожаем (если это выбор)
    }
}
