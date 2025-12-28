using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Treasure
{
    //Дополнительная логика работы сокровищ. кто куда сколько
    public class TreasurePayload
    {
        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("target")]
        public string? Target { get; set; }

        [JsonPropertyName("damage")]
        public int? Damage { get; set; }
    }
}
