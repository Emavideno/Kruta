using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Mini.Cards
{
    //Initiative - инициатива, на изображении карт в левом нижнем углу число в звездочке
    //Просто дополнительная сумма к Мощи

    // --- 1. ВЯЛАЯ ПАЛОЧКА ---
    public class LimpWandCard : ICardMini
    {
        public int CardId => 1;
        public string Name => "Limp Wand";
        public int Cost => 2;
        public int PowerBonus => 1;
        public int HealthBonus => 0;
        public int Damage => 0;
        public int DrawCount => 0;
    }

    public class BattleSaxCard : ICardMini
    {
        public int CardId => 2;
        public string Name => "Battle Sax";
        public int Cost => 3;
        public int PowerBonus => 0;
        public int HealthBonus => 0;
        public int Damage => 2;
        public int DrawCount => 0;
    }

    public class FizzleCard : ICardMini
    {
        public int CardId => 3;
        public string Name => "Fizzle";
        public int Cost => 1;
        public int PowerBonus => 0;
        public int HealthBonus => 0;
        public int Damage => 1;
        public int DrawCount => 0;
    }

    public class SnotKnightCard : ICardMini
    {
        public int CardId => 4;
        public string Name => "Snot Knight";
        public int Cost => 3;
        public int PowerBonus => 0;
        public int HealthBonus => 2;
        public int Damage => 1;
        public int DrawCount => 0;
    }

    public class TwinsCard : ICardMini
    {
        public int CardId => 5;
        public string Name => "Twins";
        public int Cost => 2;
        public int PowerBonus => 1;
        public int HealthBonus => 0;
        public int Damage => 1;
        public int DrawCount => 0;
    }

    public class WandCard : ICardMini
    {
        public int CardId => 6;
        public string Name => "Wand";
        public int Cost => 1;
        public int PowerBonus => 0;
        public int HealthBonus => 1;
        public int Damage => 0;
        public int DrawCount => 0;
    }

    public class SignCard : ICardMini
    {
        public int CardId => 7;
        public string Name => "Sign";
        public int Cost => 1;
        public int PowerBonus => 0;
        public int HealthBonus => 0;
        public int Damage => 0;
        public int DrawCount => 1; // Добор карты
    }

    public class WildMagicCard : ICardMini
    {
        public int CardId => 8;
        public string Name => "Wild Magic";
        public int Cost => 2;
        public int PowerBonus => 0;
        public int HealthBonus => 0;
        public int Damage => 2;
        public int DrawCount => 1;
    }

    public class InfernoCard : ICardMini
    {
        public int CardId => 9;
        public string Name => "Inferno";
        public int Cost => 5;
        public int PowerBonus => 0;
        public int HealthBonus => 0;
        public int Damage => 3;
        public int DrawCount => 0;
    }

    public class KrutagidonCard : ICardMini
    {
        public int CardId => 10;
        public string Name => "Krutagidon";
        public int Cost => 7;
        public int PowerBonus => 0;
        public int HealthBonus => 0;
        public int Damage => 5;
        public int DrawCount => 0;

        // Дополнительное поле для массового урона
        public int DamageToAll { get; set; } = 2;
    }
}