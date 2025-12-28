using Kruta.Protocol.Serilizations;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kruta.Shared.Mini
{
    public class PlayerMini
    {
        public int Id { get; set; } //id
        public string Nickname { get; set; } //имя игрока (которое вводит в лобби)

        public int Hp { get; set; } = 20; //хп (по правилам изначально 20)
        //public int PowerInTurn { get; set; } = 0; //сколько накопил мощи за ход (на будущее)

        public List<ICardMini> Sbros { get; set; } = new(); //сброс
        public List<ICardMini> Hand { get; set; } = new(); //карты в руке
    }
}