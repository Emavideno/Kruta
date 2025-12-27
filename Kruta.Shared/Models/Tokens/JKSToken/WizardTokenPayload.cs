using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Tokens.JKSToken
{
    //Дополнение для логики, куда как и сколько
    public class WizardTokenPayload
    {
        [JsonPropertyName("power_buff")]
        public int? PowerBuff { get; set; }

        [JsonPropertyName("draw_cards")]
        public int? DrawCards { get; set; }

        [JsonPropertyName("card_type_filter")]
        public string? CardTypeFilter { get; set; }

        [JsonPropertyName("count")]
        public int? Count { get; set; }

        [JsonPropertyName("power_amount")]
        public int? PowerAmount { get; set; }

        [JsonPropertyName("cost_limit")]
        public int? CostLimit { get; set; }
    }
}
