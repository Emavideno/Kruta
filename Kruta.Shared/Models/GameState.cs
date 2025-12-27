using Kruta.Shared.Models.Cards;
using System.Collections.Generic;
using System.Numerics;

namespace Kruta.Shared.Models
{
    // Заглушка, отражающая текущее состояние игры на столе
    public class GameState
    {
        public List<Player> Players { get; set; } = new List<Player>();
        public List<Card> Baraholka { get; set; } = new List<Card>(); // Барахолка
        public Card CurrentLegend { get; set; } // Текущая Легенда
        public int CurrentPlayerId { get; set; } // ID игрока, чей сейчас ход

        // Добавьте сюда другие общие стопки и состояния, необходимые для UI
        // Например: WildMagicStack, SlothWandsStack, DeadWizardTokensStack
    }
}