using Kruta.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Models.Cards
{
    // Шальная магия (специальная стопка)
    public class WildMagicCard : Card
    {
        public WildMagicCard()
        {
            Type = CardType.WildMagic;
            BuyCost = 3; // Каждую можно купить за 3 мощи
        }
    }
}
