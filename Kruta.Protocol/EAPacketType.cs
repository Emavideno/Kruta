using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Protocol
{
    public enum EAPacketType
    {
        Unknown,
        PlayerConnected,    // 1:0
        PlayerDisconnected, // 1:1
        PlayerListRequest,  // 2:0
        PlayerListUpdate,   // 2:1
        ToggleReady,        // 3:0 (Клиент -> Сервер)
        PlayerReadyStatus,  // 3:1 (Сервер -> Клиент: статус конкретного игрока)
        GameStarted,        // 4:0 (Сервер -> Клиент: команда на переход к игре)
        GameStateUpdate     // 4:1 (Сервер -> Клиент: обновление состояния игры)
    }
}
