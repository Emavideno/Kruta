using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.Network.Enums;
using Kruta.Shared.Network.Protocol;

namespace Kruta.Shared.Network.Messages.ServerMessages
{
    public class InteractionRequestMessage : IMessage
    {
        public MessageType Type => MessageType.InteractionRequest;

        public string AttackerName { get; set; } // Кто напал
        public string Description { get; set; }  // Текст: "Вам наносят 5 урона!"
        public int TimeToReactSeconds { get; set; } = 30; // Таймер
        public bool CanRefuse { get; set; } = true; // Можно ли нажать "Не защищаться"
    }
}
