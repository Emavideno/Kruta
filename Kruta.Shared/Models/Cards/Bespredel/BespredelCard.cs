using Kruta.Shared.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Kruta.Shared.Models.Cards.Bespredel
{
    //Беспредел
    public class BespredelCard : ICard
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("card_type")]
        public string CardType { get; set; } // "Беспредел"

        [JsonPropertyName("effect_logic")]
        public List<BespredelLogic> EffectLogic { get; set; }
    }
}
