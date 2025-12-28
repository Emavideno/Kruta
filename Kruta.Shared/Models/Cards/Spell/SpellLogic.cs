using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Spell
{
    //логика действия заклинания
    public class SpellLogic
    {
        [JsonPropertyName("step")]
        public int Step { get; set; }

        [JsonPropertyName("effect")]
        public string Effect { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        [JsonPropertyName("condition")]
        public string? Condition { get; set; }

        [JsonPropertyName("bonus_power")]
        public int? BonusPower { get; set; }

        [JsonPropertyName("target_type")]
        public string? TargetType { get; set; }

        // Урон может быть числом или строкой (как в Блеватико: "Acquired_Card_Cost")
        // Используем object или заставляем Json сериализовать в string
        [JsonPropertyName("damage")]
        public object? Damage { get; set; }

        [JsonPropertyName("trigger")]
        public string? Trigger { get; set; }

        [JsonPropertyName("filter")]
        public string? Filter { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("payload")]
        public SpellPayload? Payload { get; set; }
    }
}
