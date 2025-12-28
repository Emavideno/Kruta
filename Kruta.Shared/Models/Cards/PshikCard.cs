using Kruta.Shared.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards
{
    public class PshikCard : ICard
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } // "SIGN_TEMPLATE"

        [JsonPropertyName("title")]
        public string Title { get; set; } // "Знак"

        [JsonPropertyName("card_type")]
        public string CardType { get; set; } // "Знак"

        [JsonPropertyName("cost")]
        public int Cost { get; set; } // 0

        [JsonPropertyName("power_gems")]
        public int PowerGems { get; set; } // 0 (Базовый вклад в мощь)

        [JsonPropertyName("set_id")]
        public string SetId { get; set; } // "K"

        [JsonPropertyName("total_copies")]
        public int TotalCopies { get; set; } // 30 (Ключевое поле для генерации)

        [JsonPropertyName("power_mod")]
        public int PowerMod { get; set; } // 1 (Базовый урон или модификатор)

        [JsonPropertyName("is_attack")]
        public bool IsAttack { get; set; } // false

        // Поскольку логика null, мы можем оставить это поле как nullable List
        [JsonPropertyName("attack_logic")]
        public List<object>? AttackLogic { get; set; } // null
    }
}
