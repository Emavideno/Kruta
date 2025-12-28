using Kruta.Shared.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Creature
{
    //Тварь
    public class CreatureCard : ICard
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("card_type")]
        public string CardType { get; set; } // "Тварь"

        [JsonPropertyName("cost")]
        public int Cost { get; set; }

        [JsonPropertyName("power_gems")]
        public int PowerGems { get; set; }

        [JsonPropertyName("power_mod")]
        public int PowerMod { get; set; }

        [JsonPropertyName("entry_effect_logic")]
        public List<CreatureLogic>? EntryEffectLogic { get; set; }

        [JsonPropertyName("attack_logic")]
        public List<CreatureLogic>? AttackLogic { get; set; }

        [JsonPropertyName("defense_logic")]
        public List<CreatureLogic>? DefenseLogic { get; set; }

        [JsonPropertyName("passive_logic")]
        public List<CreatureLogic>? PassiveLogic { get; set; }

        [JsonPropertyName("on_play_trigger")]
        public List<CreatureLogic>? OnPlayTrigger { get; set; }
    }
}
