using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Models.Enums
{
    public enum CardType
    {
        // Основная колода
        Wizard,         // Волшебник
        Creature,       // Тварь
        Spell,          // Заклинание
        Treasure,       // Сокровище
        Location,       // Место
        Calamity,       // Беспредел

        // Стартовые карты
        Sign,           // Знак (Карта знаков)
        Wand,           // Палочка
        Pshik,          // Пшик (пустые карты)

        // Специальные стопки
        Familiar,       // Фамильяр
        Legend,         // Легенда
        WildMagic,      // Шальная магия
        SlothWand       // Вялая палочка
    }
}
