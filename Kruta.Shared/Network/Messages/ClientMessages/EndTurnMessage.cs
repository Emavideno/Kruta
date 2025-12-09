using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.Network.Enums;
using Kruta.Shared.Network.Protocol;

namespace Kruta.Shared.Network.Messages.ClientMessages
{
    public class EndTurnMessage : IMessage
    {
        public MessageType Type => MessageType.EndTurn;
    }
}