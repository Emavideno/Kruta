using Kruta.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Models.Cards
{
    // Беспредел (специальный класс из-за особой механики)
    public class CalamityCard : Card
    {
        public CalamityCard()
        {
            Type = CardType.Calamity;
        }

        // Дополнительные свойства, если нужны (например, цель атаки)
    }
}
