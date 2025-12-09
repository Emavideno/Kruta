using Kruta.Shared.Models.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Models.Cards
{
    // Базовый класс карты
    public abstract class Card
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public CardType Type { get; set; }

        // Стоимость для покупки (для Барахолки, Шальной магии, Фамильяра)
        public int BuyCost { get; set; } = 0;

        // Мощь, которую приносит карта при розыгрыше
        public int PowerValue { get; set; } = 0;

        // Победные очки (белая звездочка)
        public int VictoryPoints { get; set; } = 0;

        // Свойства карты
        public bool IsPermanent { get; set; } = false; // Постоянка
        public CardEffectType EffectType { get; set; } = CardEffectType.None;
    }
}
