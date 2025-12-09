using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.Network.Enums;
using Kruta.Shared.Network.Protocol;

namespace Kruta.Shared.Network.Messages.ClientMessages
{
    public class BuyCardMessage : IMessage
    {
        public MessageType Type => MessageType.BuyCard;
        public int CardId { get; set; }     // ID карты (для Барахолки)
        public BuySource Source { get; set; } // Откуда берем (Барахолка, Шальная, Фамильяр)
    }
}
