using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.Network.Enums;

namespace Kruta.Shared.Network.Protocol
{
    // Интерфейс, который обязывает каждое сообщение знать свой Тип
    public interface IMessage
    {
        MessageType Type { get; }
    }
}
