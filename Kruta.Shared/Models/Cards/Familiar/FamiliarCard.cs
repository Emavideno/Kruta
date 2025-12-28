using Kruta.Shared.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Familiar
{
    public class FamiliarCard : IOpenedCard
    {
        // Базовые свойства карты
        [JsonPropertyName("id")]
        public string Id { get; set; } // Например, "FAM_01"

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("card_type")]
        public string CardType { get; set; } // "Фамильяр"

        [JsonPropertyName("cost")]
        public int Cost { get; set; } // Всегда 6

        [JsonPropertyName("power_gems")]
        public int PowerGems { get; set; } // Сколько мощи дает при розыгрыше

        [JsonPropertyName("power_mod")]
        public int PowerMod { get; set; } // Вероятно, бонус к мощи или другой модификатор

        // Логика Пассивного Эффекта (то, что делает при разыгрывании)
        [JsonPropertyName("passive_logic")]
        public List<FamiliarEffectLogic>? PassiveLogic { get; set; }

        // Ссылка на общую логику защиты (DEF_FAM)
        [JsonPropertyName("defense_logic_ref")]
        public string DefenseLogicRef { get; set; } // "DEF_FAM"

        // Свойство, связанное с правилами игры
        /// <summary>
        /// True, если Фамильяр был куплен (за 6 мощи) и находится в колоде игрока.
        /// Изначально False.
        /// </summary>
        public bool IsActive { get; set; } = false; 
    }
}
