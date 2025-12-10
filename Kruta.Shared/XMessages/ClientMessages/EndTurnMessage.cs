using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.XProtocol;

namespace Kruta.Shared.XMessages.ClientMessages
{
    public class EndTurnMessage
    {
        // Поле 1: ID игрока, который завершил ход
        [XField(1)]
        public int PlayerId;
    }
}
