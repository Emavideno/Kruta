using Kruta.Shared.Models.Cards;
using Kruta.Shared.Models.Cards.Familiar;
using Kruta.Shared.Models.Interfaces;
using Kruta.Shared.Models.Tokens.JKSToken;
using Kruta.Shared.Services.Static;
using System.Collections.Generic;

namespace Kruta.Shared.Models
{
    // Заглушка, отражающая общедоступное состояние одного игрока
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Hp { get; set; }
        public int PowerGainedThisTurn { get; set; } // Мощь за текущий ход

        public WizardPropertyToken WizardPropertyToken { get; set; } //ЖКС
        public FamiliarCard Familiar { get; set; } //Фамильяр

        // Карты, которые видят все (тут же будут ЖКС и Фамильяр)
        public List<IOpenedCard> PublicCards { get; set; } = new();

        // Закрытое состояние (нужно для сервера/логики, но не для GameStateMessage)
        public List<ICard> Hand { get; set; } = new ();
        public List<ICard> PlayerDeck { get; set; } = new ();
        public List<ICard> Discard { get; set; } = new ();
        public List<ICard> PlayedCardsThisTurn { get; set; } = new();

        public void ShuffleDeck()
        {
            PlayerDeck = PlayerDeck.ShuffleCustom().ToList();
        }
    }
}