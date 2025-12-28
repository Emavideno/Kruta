using Kruta.Shared.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Tokens.JKSToken
{
    //ЖКС
    public class WizardPropertyToken : IOpenedCard
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("base_text")]
        public string BaseText { get; set; }

        [JsonPropertyName("is_public")]
        public bool IsPublic { get; set; }

        [JsonPropertyName("triggered_logic")]
        public List<WizardTokenLogic>? TriggeredLogic { get; set; }
    }
}
