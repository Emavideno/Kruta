using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Mini
{
    //Заглушка карт. Все карты игры будут наследоваться от ICardMini
    public class SomeCardMini : ICardMini
    {
        public int CardId { get; set; }
        public string Name { get; set; } = "Неизвестная карта";
        public int Cost { get; set; }
        public int PowerBonus { get; set; } = 0;
        public int HealthBonus { get; set; } = 0;
        public int Damage { get; set; } = 0;
        public int DrawCount { get; set; } = 0;
    }
}