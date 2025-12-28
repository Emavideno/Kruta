using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Bespredel
{
    //Логика эффектов беспредела
    public class BespredelLogic
    {
        [JsonPropertyName("step")]
        public int? Step { get; set; }

        [JsonPropertyName("effect")]
        public string Effect { get; set; }

        [JsonPropertyName("target_type")]
        public string TargetType { get; set; }

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        [JsonPropertyName("damage")]
        public object? Damage { get; set; }

        // Уникальные поля для Беспредела
        [JsonPropertyName("on_fail_effect")]
        public string? OnFailEffect { get; set; }

        [JsonPropertyName("card_source")]
        public string? CardSource { get; set; }

        [JsonPropertyName("vp_per_cost")]
        public int? VpPerCost { get; set; }

        [JsonPropertyName("draw_amount")]
        public int? DrawAmount { get; set; }

        [JsonPropertyName("draw_source")]
        public string? DrawSource { get; set; }

        [JsonPropertyName("condition")]
        public string? Condition { get; set; }

        [JsonPropertyName("destroy_target")]
        public string? DestroyTarget { get; set; }

        [JsonPropertyName("vp_gained")]
        public int? VpGained { get; set; }

        [JsonPropertyName("destroy_source")]
        public string? DestroySource { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
    }
}
