using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Familiar
{
    //МОДЕЛЬ ДЛЯ ДЕЙСТВИЯ В СПИСКЕ (Используется для DEF_FAM)
    public class FamiliarAction
    {
        [JsonPropertyName("effect")]
        public string Effect { get; set; } // Prevent_Damage_And_Effects

        [JsonPropertyName("target_type")]
        public string? TargetType { get; set; }

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        [JsonPropertyName("target_source")]
        public string? TargetSource { get; set; }

        [JsonPropertyName("new_target")]
        public string? NewTarget { get; set; }
    }
}
