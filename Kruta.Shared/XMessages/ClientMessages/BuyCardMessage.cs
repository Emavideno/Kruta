using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.XProtocol;

namespace Kruta.Shared.XMessages.ClientMessages
{
    public class BuyCardMessage
    {
        // Поле 1: ID игрока, который покупает карту (на всякий случай, если серверу нужна явная идентификация)
        [XField(1)]
        public int PlayerId;

        // Поле 2: ID карты, которую покупает игрок (может быть 0, если покупается верхняя карта колоды)
        [XField(2)]
        public int CardIdToBuy = 0;
    }
}