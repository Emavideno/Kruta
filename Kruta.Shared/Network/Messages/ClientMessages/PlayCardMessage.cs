using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.Network.Enums;
using Kruta.Shared.Network.Protocol;

namespace Kruta.Shared.Network.Messages.ClientMessages
{
    public class PlayCardMessage : IMessage
    {
        public MessageType Type => MessageType.PlayCard;
        public int CardId { get; set; }          // ID карты в руке
        public int? TargetPlayerId { get; set; } // Если карта требует цель (атака)
    }
}
