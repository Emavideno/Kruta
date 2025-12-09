using Kruta.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Models.Cards
{
    // Легенда (специальный класс из-за механики Групповой Атаки)
    public class LegendCard : Card
    {
        public string GroupAttackText { get; set; } // Текст Групповой Атаки

        public LegendCard()
        {
            Type = CardType.Legend;
            // Стоимость Легенды будет устанавливаться индивидуально (8-12)
        }
    }
}
