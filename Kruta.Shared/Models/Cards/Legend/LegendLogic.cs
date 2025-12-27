using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Legend
{
    //логика легенды
    public class LegendLogic
    {
        [JsonPropertyName("step")]
        public int Step { get; set; }

        [JsonPropertyName("effect")]
        public string Effect { get; set; }

        [JsonPropertyName("target_type")]
        public string? TargetType { get; set; }

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        [JsonPropertyName("damage")]
        public int? Damage { get; set; }

        [JsonPropertyName("condition")]
        public string? Condition { get; set; }

        [JsonPropertyName("deck_source")]
        public string? DeckSource { get; set; }

        [JsonPropertyName("deck_target")]
        public string? DeckTarget { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("optional")]
        public bool? Optional { get; set; }

        [JsonPropertyName("payload")]
        public LegendPayload? Payload { get; set; }
    }
}
