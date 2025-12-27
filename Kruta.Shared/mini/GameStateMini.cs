using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Mini
{
    public class GameStateMini
    {
        public List<PlayerMini> Players { get; set; } = new();
        public BaraholkaMini Baraholka { get; set; } = new();
        public DeckMini MainDeck { get; set; } = new();

        // ID игрока, чей сейчас ход
        public int CurrentTurnPlayerId { get; set; }
    }
}
