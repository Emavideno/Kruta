using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.Network.Enums;
using Kruta.Shared.Network.Protocol;

namespace Kruta.Shared.Network.Messages.ServerMessages
{
    public class SystemMessage : IMessage
    {
        public MessageType Type => MessageType.Notification;
        public string Text { get; set; }
        public bool IsError { get; set; } // true = красный текст, false = обычный лог
    }
}
