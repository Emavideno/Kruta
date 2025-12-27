using Kruta.Shared.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Spell
{
    public class SpellCard : ICard
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("card_type")]
        public string CardType { get; set; } // "Заклинание"

        [JsonPropertyName("cost")]
        public int Cost { get; set; }

        [JsonPropertyName("power_gems")]
        public int PowerGems { get; set; }

        [JsonPropertyName("power_mod")]
        public int PowerMod { get; set; }

        // Эффект при входе в игру (разыгрывании)
        [JsonPropertyName("entry_effect_logic")]
        public List<SpellLogic>? EntryEffectLogic { get; set; }

        // Логика атаки
        [JsonPropertyName("attack_logic")]
        public List<SpellLogic>? AttackLogic { get; set; }

        // Логика защиты
        [JsonPropertyName("defense_logic")]
        public List<SpellLogic>? DefenseLogic { get; set; }
    }
}
