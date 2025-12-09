using Kruta.Shared.Models.Cards;
using System.Collections.Generic;

namespace Kruta.Shared.Models
{
    // Заглушка, отражающая общедоступное состояние одного игрока
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CurrentHealth { get; set; }
        public int PowerGainedThisTurn { get; set; } // Мощь за текущий ход

        // Карты, которые видят все
        public List<Card> Permanents { get; set; } = new List<Card>();
        public Card InitialFamiliar { get; set; } // Купленный Фамильяр

        // Закрытое состояние (нужно для сервера/логики, но не для GameStateMessage)
        public List<Card> Hand { get; set; } = new List<Card>();
        public List<Card> Deck { get; set; } = new List<Card>();
        public List<Card> Discard { get; set; } = new List<Card>();
        public List<Card> PlayedCardsThisTurn { get; set; } = new List<Card>();
    }
}