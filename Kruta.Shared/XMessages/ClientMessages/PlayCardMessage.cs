using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.XProtocol;

namespace Kruta.Shared.XMessages.ClientMessages
{
    public class PlayCardMessage
    {
        // Поле 1: ID карты, которую разыгрывает игрок
        [XField(1)]
        public int CardId;

        // Поле 2: ID игрока, на которого направлена карта (может быть 0, если нет цели)
        [XField(2)]
        public int TargetPlayerId;
    }
}
