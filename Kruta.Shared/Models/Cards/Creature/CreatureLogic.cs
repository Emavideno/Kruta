using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Creature
{
    //Логика работы тварей
    public class CreatureLogic
    {
        [JsonPropertyName("step")]
        public int? Step { get; set; }

        [JsonPropertyName("effect")]
        public string Effect { get; set; }

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("target_type")]
        public string? TargetType { get; set; }

        [JsonPropertyName("damage")]
        public object? Damage { get; set; }

        [JsonPropertyName("condition")]
        public string? Condition { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("trigger")]
        public string? Trigger { get; set; }

        [JsonPropertyName("buff_type")]
        public string? BuffType { get; set; }

        [JsonPropertyName("duration")]
        public string? Duration { get; set; }

        [JsonPropertyName("filter")]
        public string? Filter { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("payload")]
        public CreaturePayload? Payload { get; set; }
    }
}
