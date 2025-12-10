using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.XProtocol;

namespace Kruta.Shared.XMessages.ServerMessages
{
    public struct GameState
    {
        // Здесь должны быть все поля состояния игры, используя Value Types
        // ВАЖНО: Structs лучше всего подходят для бинарных протоколов, так как они Value Types.

        [XField(1)]
        public int TurnNumber;

        [XField(2)]
        public int ActivePlayerId;

        [XField(3)]
        public int TotalPlayers;

        [XField(4)]
        public bool IsGameRunning;

        // Если вы хотите передать сложные объекты (карты, игроков), вам придется
        // сериализовать их в массив байтов и передавать через SetValueRaw/GetValueRaw.
    }

    // В XProtocol, если мы хотим передать структуру GameState, мы можем просто 
    // сериализовать ее напрямую.
    public class GameStateUpdateMessage
    {
        [XField(1)]
        public GameState CurrentState;
    }
}
