using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Spell
{
    //уточнение для работы логики заклинания, кто куда где
    public class SpellPayload
    {
        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("damage")]
        public int? Damage { get; set; }

        [JsonPropertyName("heal_amount")]
        public int? HealAmount { get; set; }

        [JsonPropertyName("target_type")]
        public string? TargetType { get; set; }
    }
}
