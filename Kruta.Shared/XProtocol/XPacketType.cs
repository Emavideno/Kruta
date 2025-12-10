using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.XProtocol
{
    // Используйте это перечисление, чтобы связать имя с бинарными Type/Subtype
    public enum XPacketType : byte
    {
        Unknown = 0,

        // --- СЕРВИСНЫЕ (Type = 1) ---
        Handshake = 10, // Type 1, Subtype 0

        // --- СООБЩЕНИЯ КЛИЕНТА (Type = 2) ---
        Auth = 20,       // Type 2, Subtype 0 (Аутентификация/Регистрация)
        PlayCard = 21,   // Type 2, Subtype 1 
        BuyCard = 22,    // Type 2, Subtype 2
        EndTurn = 23,    // Type 2, Subtype 3

        // --- СООБЩЕНИЯ СЕРВЕРА (Type = 3) ---
        GameStateUpdate = 30, // Type 3, Subtype 0
        GameOver = 31,        // Type 3, Subtype 1
        Error = 32,           // Type 3, Subtype 2
        PlayerConnected = 33, // Type 3, Subtype 3
    }
}
