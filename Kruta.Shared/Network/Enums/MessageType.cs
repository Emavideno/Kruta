using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Network.Enums
{
    public enum MessageType
    {
        // === Client -> Server (Действия) ===
        Auth,               // Вход с именем
        PlayCard,           // Сыграть карту с руки
        BuyCard,            // Купить карту (Барахолка/Шальная/Фамильяр)
        AttackLegend,       // Напасть на легенду
        ReactionResponse,   // Ответ на запрос (Защищаюсь/Нет)
        EndTurn,            // Конец хода
        DestroyCard,        // Уничтожение карты

        // === Server -> Client (События) ===
        Welcome,            // Успешный вход + твой ID
        GameStateUpdate,    // Полное обновление стола
        InteractionRequest, // "Тебя бьют! Выбери защиту"
        Notification,       // Текстовое сообщение (лог)
        Error,              // Ошибка (нельзя купить, нет маны)
        GameOver
    }
}
