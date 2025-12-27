using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Mini
{
    public interface ICardMini
    {
        public int CardId { get; set; } //id (если не нужно - убери)
        public int Cost { get; set; } //стоимость

        //public int Power { get; set; } //сколько дает мощи (на будущее)
        //public string pathToImage { get; set; } //ссылка на картинку (на будущее)
    }
}