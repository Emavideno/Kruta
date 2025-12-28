using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Familiar
{
    //Описание одного действия фамильяра
    public class FamiliarEffectLogic
    {
        [JsonPropertyName("step")]
        public int? Step { get; set; } // Для многошаговых эффектов (FAM_04, FAM_08)

        [JsonPropertyName("effect")]
        public string Effect { get; set; } // Что делать (Attack_Immediate, Heal_Life)

        [JsonPropertyName("trigger_event")]
        public string? TriggerEvent { get; set; } // Когда срабатывает (On_Attacked)

        [JsonPropertyName("condition")]
        public string? Condition { get; set; } // Условие срабатывания (Familiar_Discarded, Destroy_One_From_Hand)

        [JsonPropertyName("action_list")]
        public List<FamiliarAction>? ActionList { get; set; } // Для сложных, многошаговых защитных эффектов

        [JsonPropertyName("target_type")]
        public string? TargetType { get; set; }

        [JsonPropertyName("damage")]
        public int? Damage { get; set; }

        [JsonPropertyName("amount")]
        public int? Amount { get; set; }

        [JsonPropertyName("deck_source")]
        public string? DeckSource { get; set; }

        [JsonPropertyName("destroy_limit")]
        public int? DestroyLimit { get; set; }

        // Дополнительные свойства для токенов
        [JsonPropertyName("token_type")]
        public string? TokenType { get; set; }
        [JsonPropertyName("source")]
        public string? Source { get; set; }
        [JsonPropertyName("target")]
        public string? Target { get; set; }

        // Для FAM_02
        [JsonPropertyName("payload")]
        public FamiliarPayload? Payload { get; set; }

        // Для FAM_11
        [JsonPropertyName("optional_play")]
        public int? OptionalPlay { get; set; }
    }
}
