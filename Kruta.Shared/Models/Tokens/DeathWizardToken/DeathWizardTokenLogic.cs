using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Tokens.DeathWizardToken
{
    public class DeathWizardTokenLogic
    {
        [JsonPropertyName("action_type")]
        public string ActionType { get; set; } // Например, "Damage_Killer"

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        [JsonPropertyName("health_value")]
        public int? HealthValue { get; set; }

        [JsonPropertyName("card_id")]
        public string? CardId { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("target")]
        public string? Target { get; set; }

        [JsonPropertyName("destination")]
        public string? Destination { get; set; }

        [JsonPropertyName("is_optional")]
        public bool IsOptional { get; set; } = false;
    }
}
