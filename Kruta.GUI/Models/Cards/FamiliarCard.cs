using Kruta.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Models.Cards
{
    // Фамильяр (специальный класс)
    public class FamiliarCard : Card
    {
        // ID Колдуна, к которому привязан фамильяр изначально
        public int InitialWizardId { get; set; }

        public FamiliarCard()
        {
            Type = CardType.Familiar;
            BuyCost = 6; // Фамильяра можно купить за 6 мощи
        }
    }
}
