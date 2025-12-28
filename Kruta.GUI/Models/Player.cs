using Kruta.Shared.Models.Cards;
using Kruta.Shared.Models.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.GUI.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int CurrentHealth { get; set; } = 20;

        // --- Личные Колоды ---
        // Личная колода (для взятия карт)
        public List<Card> Deck { get; set; } = new List<Card>();
        // Карты на руке
        public List<Card> Hand { get; set; } = new List<Card>();
        // Личная стопка сброса (открыта)
        public List<Card> Discard { get; set; } = new List<Card>();
        // Карты "Постоянка", которые находятся в игре
        public List<Card> Permanents { get; set; } = new List<Card>();

        // --- Свойства Колдуна ---
        // Колдунское Свойство (открыто)
        public WizardPropertyToken PropertyToken { get; set; }
        // Фамильяр, привязанный к колдуну (пока не куплен, виден всем)
        public FamiliarCard InitialFamiliar { get; set; }

        // --- Специальные объекты ---
        // Жетоны Дохлых Колдунов (открыты)
        public List<DeadWizardToken> DeadWizardTokens { get; set; } = new List<DeadWizardToken>();
        // Владеет ли игрок Главным Призом Крутагидона
        public bool ControlsKrutagidonPrize { get; set; } = false;

        // --- Временные значения для текущего хода ---
        public int PowerGainedThisTurn { get; set; } = 0; // Накопленная мощь
        public List<Card> PlayedCardsThisTurn { get; set; } = new List<Card>(); // Сыгранные карты
    }
}
