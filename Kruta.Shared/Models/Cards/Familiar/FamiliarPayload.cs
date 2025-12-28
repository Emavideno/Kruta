using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Familiar
{
    //МОДЕЛЬ ДЛЯ ВЛОЖЕННЫХ ПАРАМЕТРОВ(Payload)
    public class FamiliarPayload
    {
        [JsonPropertyName("optional_action")]
        public string? OptionalAction { get; set; } // Destroy_One

        [JsonPropertyName("keep_one_on_top")]
        public bool? KeepOneOnTop { get; set; } // True
    }
}
