using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Legend
{
    //дополнение к описанию логики, кто куда где
    public class LegendPayload
    {
        [JsonPropertyName("card_selection")]
        public string? CardSelection { get; set; }

        [JsonPropertyName("target_discard")]
        public string? TargetDiscard { get; set; }

        [JsonPropertyName("store_cost_multiplier")]
        public int? StoreCostMultiplier { get; set; }

        [JsonPropertyName("target_owner")]
        public string? TargetOwner { get; set; }
    }
}
