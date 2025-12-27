using Kruta.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Models.Cards
{
    // Вялая палочка (специальная стопка)
    public class SlothWandCard : Card
    {
        public SlothWandCard()
        {
            Type = CardType.SlothWand;
            VictoryPoints = -1; // Обычно минус ПО
            // Вялые палочки нельзя купить, BuyCost = 0
        }
    }
}
