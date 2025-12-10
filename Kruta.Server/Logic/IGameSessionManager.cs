// Kruta.Server/Logic/IGameSessionManager.cs

using Kruta.Shared.XProtocol; // Нужно для XPacket в BroadcastPacket

namespace Kruta.Server.Logic
{
    public interface IGameSessionManager
    {
        /// <summary>
        /// Удаляет клиента из коллекции активных соединений/сессии.
        /// </summary>
        void RemoveClient(int clientId);

        /// <summary>
        /// Регистрирует нового игрока в логике игры и рассылает уведомление всем.
        /// (Этот метод вызывает ошибку, если его нет в интерфейсе!)
        /// </summary>
        void RegisterNewPlayer(int clientId, string playerName);

        /// <summary>
        /// Рассылает пакет всем активным клиентам.
        /// </summary>
        void BroadcastPacket(XPacket packet);

        // Тут можно добавить другие методы, например:
        // void ProcessPlayCard(int clientId, int cardId, int targetId);
    }
}