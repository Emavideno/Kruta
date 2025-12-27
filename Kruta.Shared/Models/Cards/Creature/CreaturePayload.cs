using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Creature
{
    //Дополнение логики работы тварей. кто куда где
    public class CreaturePayload
    {
        [JsonPropertyName("destination")]
        public string? Destination { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("target")]
        public string? Target { get; set; }

        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }

        [JsonPropertyName("draw_amount")]
        public int? DrawAmount { get; set; }

        [JsonPropertyName("card_filter")]
        public string? CardFilter { get; set; }

        [JsonPropertyName("choice_required")]
        public bool? ChoiceRequired { get; set; }
    }
}
