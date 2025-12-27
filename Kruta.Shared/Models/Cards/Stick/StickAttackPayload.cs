using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Stick
{
    //Вложенные параметры для логики атаки (уточняющие более точно)
    public class StickAttackPayload
    {
        [JsonPropertyName("condition")]
        public string? Condition { get; set; } // Target_Died

        [JsonPropertyName("action")]
        public string? Action { get; set; } // Draw_Cards

        [JsonPropertyName("amount")]
        public int? Amount { get; set; } // 2
    }
}
