using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.Network.Enums;
using Kruta.Shared.Network.Protocol;

namespace Kruta.Shared.Network.Messages.ClientMessages
{
    public class ReactionMessage : IMessage
    {
        public MessageType Type => MessageType.ReactionResponse;
        public bool IsDefending { get; set; }    // true = кидаю защиту, false = принимаю урон
        public int? DefenseCardId { get; set; }  // ID карты защиты (если IsDefending = true)
    }
}
