using Kruta.Shared.Mini;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Mini
{
    //общая колода
    public class DeckMini
    {
        public List<ICardMini> Cards { get; set; } = new(); //карты в общей колоде
    }
}