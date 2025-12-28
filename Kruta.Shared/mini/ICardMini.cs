using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Mini
{
    public interface ICardMini
    {
        int CardId { get; }
        string Name { get; }
        int Cost { get; }
        int PowerBonus { get; }  // Добавляет мощь
        int HealthBonus { get; } // Лечит (Initiative)
        int Damage { get; }      // Прямой урон
        int DrawCount { get; }   // Добор карт
    }
}