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
        PlayerHpUpdate,     // 3:2 (Type 3 (Lobby/Status), Subtype 2 (HP Update))
        GameStarted,        // 4:0 (Сервер -> Клиент: команда на переход к игре)
        GameStateUpdate,     // 4:1 (Сервер -> Клиент: обновление состояния игры)
        TurnAction,          //5: Ход. Подтип 0: Конец хода (от клиента)
        TurnStatus,           //Тип 5: Ход. Подтип 1: Начало хода (от сервера конкретному игроку)
        AttackAction,            //5:2
        PlayCardAction,         //6:1
    }
}
