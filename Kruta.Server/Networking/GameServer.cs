using Kruta.Server.Logic; // Для IGameSessionManager
using Kruta.Server.Networking; // Для ClientConnection
using System;
using System.Collections.Concurrent; // Для ConcurrentDictionary
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Kruta.Shared.XProtocol; // Для XPacket
using Kruta.Shared.XMessages.ServerMessages; // Для PlayerConnectedMessage

namespace Kruta.Server.Networking
{
    // !!! ПРЕДПОЛАГАЕТСЯ, ЧТО ИНТЕРФЕЙС IGameSessionManager ОПРЕДЕЛЕН В Kruta.Server.Logic !!!
    // Например:
    // namespace Kruta.Server.Logic { public interface IGameSessionManager { void RemoveClient(int clientId); void RegisterNewPlayer(int clientId, string playerName); void BroadcastPacket(XPacket packet); } }

    public class GameServer : IGameSessionManager
    {
        private TcpListener _listener;
        private readonly int _port = 13000;
        private bool _isRunning = true;

        // 1. ПОЛЕ ДЛЯ ГЕНЕРАЦИИ УНИКАЛЬНОГО ID
        private int _nextClientId = 1;

        // 2. Коллекция для хранения активных соединений (опционально, но полезно)
        private ConcurrentDictionary<int, ClientConnection> _connections =
            new ConcurrentDictionary<int, ClientConnection>();

        // 3. Коллекция для хранения активных ИГРОКОВ (Player)
        private ConcurrentDictionary<int, Player> _activePlayers =
            new ConcurrentDictionary<int, Player>();

        public GameServer()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine($"[SERVER] Запущен и прослушивает порт {_port}...");

            try
            {
                while (_isRunning)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine($"[SERVER] Принят новый клиент: {client.Client.RemoteEndPoint}");

                    // 3. Запускаем обработку соединения в отдельной задаче
                    _ = HandleClientConnection(client);
                }
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                // Это ожидаемо при вызове Stop()
                Console.WriteLine("[SERVER] Прослушивание остановлено.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Ошибка в цикле Accept: {ex.Message}");
            }
            finally
            {
                _listener.Stop();
            }
        }

        private async Task HandleClientConnection(TcpClient client)
        {
            // --- ГЕНЕРАЦИЯ ID И ВЫЗОВ КОНСТРУКТОРА С НОВЫМИ АРГУМЕНТАМИ ---

            // Получаем уникальный ID (используем Interlocked для безопасности потоков)
            int clientId = System.Threading.Interlocked.Increment(ref _nextClientId);

            var connection = new ClientConnection(client, clientId, this);

            // Добавляем соединение в коллекцию
            _connections.TryAdd(clientId, connection);

            await connection.ProcessAsync();
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();

            // 3. Дополнительно: Закрытие всех активных соединений
            foreach (var connection in _connections.Values)
            {
                connection.Close();
            }

            _connections.Clear();
            _activePlayers.Clear();

            // (Опционально) Сброс счетчика ID
            _nextClientId = 1;

            Console.WriteLine("[SERVER] Все соединения закрыты. Сервер остановлен.");
        }

        // ==========================================================
        // === РЕАЛИЗАЦИЯ IGameSessionManager (Методы для ClientConnection) ===
        // ==========================================================

        // 1. Метод для удаления клиента (вызывается ClientConnection при отключении)
        // Kruta.Server/Networking/GameServer.cs

        public void RemoveClient(int clientId)
        {
            // 1. Удаление соединения
            if (_connections.TryRemove(clientId, out var connection))
            {
                Console.WriteLine($"[MANAGER] Соединение клиента {clientId} удалено.");
            }

            // 2. УДАЛЕНИЕ ОБЪЕКТА PLAYER
            if (_activePlayers.TryRemove(clientId, out var player))
            {
                Console.WriteLine($"[MANAGER] Игрок {player.Name} (ID: {player.Id}) удален из активных игроков.");

                // !!! ДОБАВЛЕНО: УВЕДОМЛЕНИЕ ОСТАЛЬНЫХ ИГРОКОВ !!!
                var disconnectMsg = new PlayerDisconnectedMessage
                {
                    PlayerId = player.Id // Указываем ID игрока, который отключился
                };
                var xpacket = XPacketConverter.Serialize(XPacketType.PlayerDisconnected, disconnectMsg);
                BroadcastPacket(xpacket);
            }
        }

        // 2. Метод для рассылки пакета всем клиентам
        public void BroadcastPacket(XPacket packet)
        {
            // Отправка пакета всем активным соединениям в коллекции
            foreach (var connection in _connections.Values)
            {
                connection.QueuePacketSend(packet);
            }
        }

        // 3. Метод для регистрации нового игрока и рассылки уведомлений (вызывается из ProcessAuth)
        public void RegisterNewPlayer(int clientId, string playerName)
        {
            // 1. Генерируем ID игрока (можно использовать ClientId, если игра 1:1)
            // Сейчас используем ClientId, чтобы упростить связь
            int playerId = clientId;

            // 2. СОЗДАНИЕ И СОХРАНЕНИЕ ОБЪЕКТА PLAYER
            var newPlayer = new Player(
                id: playerId,
                name: playerName,
                clientId: clientId // Связываем Player с его сетевым ClientId
            );

            // Добавляем игрока в коллекцию
            if (!_activePlayers.TryAdd(playerId, newPlayer))
            {
                // В случае ошибки (например, если ID уже существует)
                Console.WriteLine($"[ERROR] Игрок с ID {playerId} уже существует.");
                return;
            }

            Console.WriteLine($"[MANAGER] {newPlayer.ToString()} зарегистрирован и добавлен.");

            // 3. Создание пакета PlayerConnected (рассылка)
            var msg = new PlayerConnectedMessage
            {
                PlayerId = newPlayer.Id,
                SlotIndex = newPlayer.Id,
                PlayerName = newPlayer.Name
            };

            var xpacket = XPacketConverter.Serialize(XPacketType.PlayerConnected, msg);
            msg.SerializeString(xpacket);

            // 4. Рассылка пакета всем клиентам
            BroadcastPacket(xpacket);
        }
    }
}