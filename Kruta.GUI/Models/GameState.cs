using Kruta.Shared.Models.Cards;
using Kruta.Shared.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.GUI.Models
{
    public class GameState
    {
        public List<Player> Players { get; set; } = new List<Player>();

        // --- Общие Колоды ---
        // Основная колода (закрыта)
        public List<Card> MainDeck { get; set; } = new List<Card>();
        // Барахолка (5 открытых карт)
        public List<Card> Baraholka { get; set; } = new List<Card>();

        // Стопка Легенд (закрыта, кроме верхней карты)
        public List<LegendCard> LegendStack { get; set; } = new List<LegendCard>();
        // Верхняя карта легенд (видна всем)
        public LegendCard CurrentLegend { get; set; }

        // Стопка Шальной магии (открыта)
        public List<WildMagicCard> WildMagicStack { get; set; } = new List<WildMagicCard>();
        // Стопка Вялых палочек (открыта, купить нельзя)
        public List<SlothWandCard> SlothWandsStack { get; set; } = new List<SlothWandCard>();

        // Стопка Жетон Дохлого Колдуна (ЖДК) (закрыта)
        public List<DeadWizardToken> DeadWizardTokensStack { get; set; } = new List<DeadWizardToken>();

        // --- Стопки Уничтоженных Карт ---
        // Уничтоженные карты, кроме Беспредела
        public List<Card> DestroyedCards { get; set; } = new List<Card>();
        // Уничтоженные карты Беспредела (важен порядок)
        public Stack<CalamityCard> CalamityDiscardStack { get; set; } = new Stack<CalamityCard>();

        // --- Текущий Ход ---
        public int CurrentPlayerId { get; set; } = 0;
        public int TurnNumber { get; set; } = 0;
        public bool IsGameEndConditionMet { get; set; } = false; // Условие завершения игры
        public bool IsLegendDefeatedThisTurn { get; set; } = false; // Можно победить только 1 легенду за ход

        // Глобальный приз
        public KrutagidonPrize KrutagidonPrize { get; set; } = new KrutagidonPrize();

        public Player GetCurrentPlayer()
        {
            return Players.FirstOrDefault(p => p.Id == CurrentPlayerId);
        }
    }
}
