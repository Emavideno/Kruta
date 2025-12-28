using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Mini
{
    //Заглушка карт. Все карты игры будут наследоваться от ICardMini
    public class SomeCardMini : ICardMini // Должно быть так
    {
        public int CardId { get; set; }
        public int Cost { get; set; }
        public string Name { get; set; } = "Заглушка";
        public int Initiative { get; set; }
        public string Description { get; set; }
    }
}