using Kruta.Shared.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Legend
{
    public class LegendCard : ICard
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("card_type")]
        public string CardType { get; set; } // "Легенда"

        [JsonPropertyName("default_power")]
        public int DefaultPower { get; set; } // Сила, которую нужно набрать для победы

        [JsonPropertyName("trophy_value")]
        public int TrophyValue { get; set; } // ПО за победу

        [JsonPropertyName("entry_effect_logic")]
        public List<LegendLogic>? EntryEffectLogic { get; set; }

        [JsonPropertyName("group_attack_logic")]
        public List<LegendLogic>? GroupAttackLogic { get; set; }

        // Реализация интерфейса ICard (Легенды обычно не имеют стоимости покупки)
        public int Cost => 0;
        public int PowerGems => 0;
        public int PowerMod => 0;
    }
}
