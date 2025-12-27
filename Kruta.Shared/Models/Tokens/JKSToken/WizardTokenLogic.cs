using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Tokens.JKSToken
{
    //Описание одного правила/эффекта у ЖКС
    public class WizardTokenLogic
    {
        [JsonPropertyName("trigger_event")]
        public string TriggerEvent { get; set; }

        [JsonPropertyName("effect")]
        public string Effect { get; set; }

        [JsonPropertyName("condition")]
        public string? Condition { get; set; }

        [JsonPropertyName("condition_amount")]
        public int? ConditionAmount { get; set; }

        [JsonPropertyName("card_type_filter")]
        public string? CardTypeFilter { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("source_value")]
        public string? SourceValue { get; set; }

        [JsonPropertyName("target_deck")]
        public string? TargetDeck { get; set; }

        [JsonPropertyName("target_source")]
        public string? TargetSource { get; set; }

        [JsonPropertyName("target_destination")]
        public string? TargetDestination { get; set; }

        [JsonPropertyName("optional")]
        public bool? Optional { get; set; }

        [JsonPropertyName("life_cost")]
        public int? LifeCost { get; set; }

        [JsonPropertyName("draw_amount")]
        public int? DrawAmount { get; set; }

        [JsonPropertyName("max_uses_per_turn")]
        public int? MaxUsesPerTurn { get; set; }

        // ИСПОЛЬЗУЕМ НОВОЕ ИМЯ: WizardTokenPayload
        [JsonPropertyName("payload")]
        public WizardTokenPayload? Payload { get; set; }

        // ИСПОЛЬЗУЕМ НОВОЕ ИМЯ: WizardTokenPayload
        [JsonPropertyName("payloads")]
        public List<WizardTokenPayload>? Payloads { get; set; }
    }
}
