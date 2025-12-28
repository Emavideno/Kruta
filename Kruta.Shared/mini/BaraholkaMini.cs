using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Mini
{
    //барахолка пополняется из общей колоды, DeckMini, в начале каждого хода игрока
    //То есть в начале хода каждого игрока в Барахолке должно находиться 5 карт
    //Взятых из общей колоды DeckMini
    public class BaraholkaMini
    {
        public List<ICardMini> Cards { get; set; } //максимум может быть 5 карт (по правилам)
    }
}