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
        public int CardId { get; set; } = 1;
        public int Cost { get; set; } = 0;
        public string Name { get; set; } = "Вялая палочка";
        public int Initiative { get; set; } = -1;
        public string Description { get; set; } = "(Эффекта нет.)";
    }

    // --- 2. БОЕВАЯ САКСЕКИРА ---
    public class BattleSaxCard : ICardMini
    {
        public int CardId { get; set; } = 2;
        public int Cost { get; set; } = 5;
        public string Name { get; set; } = "Боевая Саксекира";
        public int Initiative { get; set; } = 1;
        public int PowerBonus { get; set; } = 2; // +2 мощи
        public int DamageToSides { get; set; } = 5; // Урон левому и правому
        public string Description { get; set; } = "Нанеси 5 урона левому и правому врагам.";
    }

    // --- 3. ПШИК ---
    public class FizzleCard : ICardMini
    {
        public int CardId { get; set; } = 3;
        public int Cost { get; set; } = 0;
        public string Name { get; set; } = "Пшик";
        public int Initiative { get; set; } = 0;
        public string Description { get; set; } = "(Эффекта нет.)";
    }

    // --- 4. РЫЦАРЬ-СОПЛЕНОСЕЦ ---
    public class SnotKnightCard : ICardMini
    {
        public int CardId { get; set; } = 4;
        public int Cost { get; set; } = 2;
        public string Name { get; set; } = "Рыцарь-Сопленосец";
        public int Initiative { get; set; } = 1;
        public int PowerBonus { get; set; } = 2; // +2 мощи
    }

    // --- 5. ДВОЙНЯШКИ ---
    public class TwinsCard : ICardMini
    {
        public int CardId { get; set; } = 5;
        public int Cost { get; set; } = 5;
        public string Name { get; set; } = "Двойняшки";
        public int Initiative { get; set; } = 1;
        public int CardsToDraw { get; set; } = 2; // Возьми 2 карты
    }

    // --- 6. ПАЛОЧКА ---
    public class WandCard : ICardMini
    {
        public int CardId { get; set; } = 6;
        public int Cost { get; set; } = 0;
        public string Name { get; set; } = "Палочка";
        public int Initiative { get; set; } = 0;
        public int PowerBonus { get; set; } = 1; // +1 мощь
        public int Damage { get; set; } = 1; // Нанеси 1 урон
    }

    // --- 7. ЗНАК ---
    public class SignCard : ICardMini
    {
        public int CardId { get; set; } = 7;
        public int Cost { get; set; } = 0;
        public string Name { get; set; } = "Знак";
        public int Initiative { get; set; } = 0;
        public int PowerBonus { get; set; } = 1; // +1 мощь
    }

    // --- 8. ШАЛЬНАЯ МАГИЯ ---
    public class WildMagicCard : ICardMini
    {
        public int CardId { get; set; } = 8;
        public int Cost { get; set; } = 3;
        public string Name { get; set; } = "Шальная магия";
        public int Initiative { get; set; } = 1;

        // FALSE = +2 мощи (по умолчанию)
        // TRUE = Сыграть верхнюю карту (в твоем упрощении - взять карту)
        public bool IsAlternativeEffect { get; set; } = false;

        public int PowerBonus { get; set; } = 2;
    }

    // --- 9. ОГНИЩЕ ---
    public class InfernoCard : ICardMini
    {
        public int CardId { get; set; } = 9;
        public int Cost { get; set; } = 5;
        public string Name { get; set; } = "Огнище";
        public int Initiative { get; set; } = 2;
        public int PowerBonus { get; set; } = 3; // +3 мощи
    }

    // --- 10. КРУТАГИДОН! ---
    public class KrutagidonCard : ICardMini
    {
        public int CardId { get; set; } = 10;
        public int Cost { get; set; } = 7;
        public string Name { get; set; } = "Крутагидон!";
        public int Initiative { get; set; } = 2;
        public int PowerBonus { get; set; } = 3; // +3 мощи
        public int DamageToAll { get; set; } = 7; // 7 урона каждому врагу
    }
}