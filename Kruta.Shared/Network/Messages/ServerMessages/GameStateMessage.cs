using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.Models; // Убедись, что тут есть класс GameState
using Kruta.Shared.Network.Enums;
using Kruta.Shared.Network.Protocol;

namespace Kruta.Shared.Network.Messages.ServerMessages
{
    public class GameStateMessage : IMessage
    {
        public MessageType Type => MessageType.GameStateUpdate;
        public GameState State { get; set; }
    }
}
