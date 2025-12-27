using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Stick
{
    //Описание логики атаки
    public class StickAttackLogic
    {
        [JsonPropertyName("step")]
        public int? Step { get; set; }

        [JsonPropertyName("effect")]
        public string Effect { get; set; } // Damage_Target_Conditional_Draw

        [JsonPropertyName("target_type")]
        public string? TargetType { get; set; } // Selected_Enemy

        [JsonPropertyName("damage")]
        public int? Damage { get; set; } // 1

        [JsonPropertyName("payload")]
        public StickAttackPayload? Payload { get; set; }
    }
}
