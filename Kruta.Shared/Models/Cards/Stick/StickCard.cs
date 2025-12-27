using Kruta.Shared.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Stick
{
    public class StickCard : ICard
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } // "WAND_TEMPLATE"

        [JsonPropertyName("title")]
        public string Title { get; set; } // "Палочка"

        [JsonPropertyName("card_type")]
        public string CardType { get; set; } // "Палочка"

        [JsonPropertyName("cost")]
        public int Cost { get; set; } // 0

        [JsonPropertyName("power_gems")]
        public int PowerGems { get; set; } // 0 (Не дает мощи)

        [JsonPropertyName("set_id")]
        public string SetId { get; set; } // "K"

        [JsonPropertyName("total_copies")]
        public int TotalCopies { get; set; } // 5 (Ключевое поле для инициализации)

        [JsonPropertyName("power_mod")]
        public int PowerMod { get; set; } // 1 (Базовый урон или модификатор)

        [JsonPropertyName("is_attack")]
        public bool IsAttack { get; set; } // true

        [JsonPropertyName("attack_logic")]
        public List<StickAttackLogic>? AttackLogic { get; set; }
    }
}
