using Kruta.Shared.Models.Cards.Creature;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Treasure
{
    //Логика работы сокровищ
    public class TreasureLogic
    {
        [JsonPropertyName("step")]
        public int? Step { get; set; }

        [JsonPropertyName("effect")]
        public string Effect { get; set; }

        [JsonPropertyName("damage")]
        public object? Damage { get; set; } // Может быть числом или строкой

        [JsonPropertyName("condition")]
        public string? Condition { get; set; }

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("target_type")]
        public string? TargetType { get; set; }

        [JsonPropertyName("trigger")]
        public string? Trigger { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("card_filter")]
        public string? CardFilter { get; set; }

        [JsonPropertyName("action")]
        public string? Action { get; set; }

        [JsonPropertyName("buff_type")]
        public string? BuffType { get; set; }

        [JsonPropertyName("bonus_power")]
        public int? BonusPower { get; set; }

        [JsonPropertyName("payload")]
        public TreasurePayload? Payload { get; set; }
    }
}
