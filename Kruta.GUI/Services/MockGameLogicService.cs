using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared;
using Kruta.GUI.Models;
using Kruta.Shared.Models.Cards;
using Kruta.Shared.Models.Enums;

namespace Kruta.GUI.Services
{
    public class MockGameLogicService
    {
        private int _cardIdCounter = 1;

        // Создание тестового состояния игры
        public GameState CreateInitialMockState(string playerName)
        {
            var player = new Player
            {
                Id = 1,
                Name = playerName,
                CurrentHealth = 20,
                // Начальный набор карт: 6 Знаков, 1 Палочка, 3 Пшика
                Deck = GetInitialDeck(),
                InitialFamiliar = new FamiliarCard { Id = 100, Name = "Тестовый Фамильяр", BuyCost = 6, Text = "Дает +1 мощь каждый ход." }
            };

            // Берем 5 карт в руку
            player.Hand = player.Deck.Take(5).ToList();
            player.Deck = player.Deck.Skip(5).ToList();

            var otherPlayer = new Player
            {
                Id = 2,
                Name = "Бот-Соперник",
                CurrentHealth = 20
            };

            var gameState = new GameState
            {
                Players = new List<Player> { player, otherPlayer },
                MainDeck = GetMockMainDeck(),
                Baraholka = GetMockBaraholka(),
                CurrentLegend = new LegendCard { Name = "Вум", BuyCost = 8, GroupAttackText = "Все получают 2 урона." },
                CurrentPlayerId = 1 // Начинает наш игрок
            };

            return gameState;
        }

        private List<Card> GetInitialDeck()
        {
            var deck = new List<Card>();
            // 6 карт знаков
            for (int i = 0; i < 6; i++) deck.Add(new StarterCard { Id = _cardIdCounter++, Name = "Знак Мощи", PowerValue = 1, Type = CardType.Sign, Text = "+1 Мощь." });
            // 1 карта палочки
            deck.Add(new StarterCard { Id = _cardIdCounter++, Name = "Деревянная Палочка", PowerValue = 0, Type = CardType.Wand, Text = "Может дать 1 Мощь, если сброшена." });
            // 3 карты пшиков
            for (int i = 0; i < 3; i++) deck.Add(new StarterCard { Id = _cardIdCounter++, Name = "Пшик", PowerValue = 0, Type = CardType.Pshik, Text = "Ничего не дает." });
            return deck.OrderBy(x => Guid.NewGuid()).ToList(); // Перемешивание
        }

        private List<Card> GetMockMainDeck()
        {
            var deck = new List<Card>();
            deck.Add(new MainDeckCard { Id = _cardIdCounter++, Name = "Огненный шар", BuyCost = 3, PowerValue = 2, EffectType = CardEffectType.Attack, Type = CardType.Spell });
            deck.Add(new CalamityCard { Id = _cardIdCounter++, Name = "Космический Хаос", Text = "Нанесите 3 урона всем игрокам." });
            // ... добавить больше карт для имитации
            return deck;
        }

        private List<Card> GetMockBaraholka()
        {
            var baraholka = new List<Card>();
            baraholka.Add(new MainDeckCard { Id = _cardIdCounter++, Name = "Великий Волшебник", BuyCost = 5, PowerValue = 3, VictoryPoints = 1, Type = CardType.Wizard });
            baraholka.Add(new MainDeckCard { Id = _cardIdCounter++, Name = "Башня Защиты", BuyCost = 4, PowerValue = 0, EffectType = CardEffectType.Defense, IsPermanent = true, Type = CardType.Location });
            baraholka.Add(new MainDeckCard { Id = _cardIdCounter++, Name = "Золотой Сундук", BuyCost = 6, PowerValue = 1, VictoryPoints = 3, Type = CardType.Treasure });
            baraholka.Add(new MainDeckCard { Id = _cardIdCounter++, Name = "Болотная Тварь", BuyCost = 2, PowerValue = 1, EffectType = CardEffectType.Attack, Type = CardType.Creature });
            baraholka.Add(new MainDeckCard { Id = _cardIdCounter++, Name = "Артефакт Мощи", BuyCost = 4, PowerValue = 3, Type = CardType.Treasure });
            return baraholka;
        }
    }
}
