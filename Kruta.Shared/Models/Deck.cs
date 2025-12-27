using Kruta.Shared.Models.Cards.Bespredel;
using Kruta.Shared.Models.Cards.Creature;
using Kruta.Shared.Models.Cards.Spell;
using Kruta.Shared.Models.Cards.Treasure;
using Kruta.Shared.Models.Interfaces;
using Kruta.Shared.Services;
using Kruta.Shared.Services.Static;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Models
{
    public class Deck
    {
        public List<ICard> Cards { get; set; } = new();

        public Deck()
        {
            InitDeck();
        }

        private void InitDeck() //Логика заполнения всей колоды
        {
            //инициализация всех сервисов нужных карт, которые должны находиться в колоде
            SpellService spellService = new SpellService(); //заклинания
            CreatureService creatureService = new CreatureService(); //твари
            TreasureService treasureService = new TreasureService(); //сокровища
            BespredelService bespredelService = new BespredelService(); //беспределы

            //получение (копий) всех колод нужных карт
            List<SpellCard> spellsFromSpellService = spellService.GetAllSpells();
            List<CreatureCard> creaturesFromCreatureService = creatureService.GetAllCreatures();
            List<TreasureCard> treasureCardsFromTreasureService = treasureService.GetAllTreasures();
            List<BespredelCard> bespredelCardsFromBespredelService = bespredelService.GetAllBespredel();


            //добавление всех колод нужных карт в стопку общей колоды
            Cards.AddRange(spellsFromSpellService); //добавление всех карт заклинаний в колоду
            Cards.AddRange(creaturesFromCreatureService); //добавление всех карт тварей в колоду
            Cards.AddRange(treasureCardsFromTreasureService); //добавление всех карт сокровищ в колоду
            Cards.AddRange(bespredelCardsFromBespredelService); //добавление всех карт беспределов в колоду

            Cards.ShuffleCustom(); //перемешиваем готовую колоду

        }

        public void ShuffleDeck()
        {
            // Используем ShuffleService
            Cards = Cards.ShuffleCustom().ToList();
        }

        //Выдача карты из колоды
        public ICard GetCardFromDeck()
        {
            if (Cards.Count > 0)
            {
                ICard cardForReturn = Cards[0];
                Cards.Remove(cardForReturn);

                return cardForReturn;
            }
            else return null;
        }
    }
}
