using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.Network.Enums;
using Kruta.Shared.Network.Protocol;
using Kruta.Shared.Models;

namespace Kruta.Shared.Network.Messages.ServerMessages
{
    public class GameOverMessage : IMessage
    {
        public MessageType Type => MessageType.GameOver;
        public string WinnerName { get; set; }
        public List<PlayerScore> FinalScores { get; set; } // Класс Score должен быть в Shared/Models
    }
}
