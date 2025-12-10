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

            Console.WriteLine("[SERVER] Все соединения закрыты. Сервер остановлен.");
        }

        // ==========================================================
        // === РЕАЛИЗАЦИЯ IGameSessionManager (Методы для ClientConnection) ===
        // ==========================================================

        // 1. Метод для удаления клиента (вызывается ClientConnection при отключении)
        public void RemoveClient(int clientId)
        {
            if (_connections.TryRemove(clientId, out var connection))
            {
                Console.WriteLine($"[MANAGER] Клиент {clientId} удален из активных соединений.");
                // Дополнительная логика: уведомление других игроков об отключении
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
            Console.WriteLine($"[MANAGER] Игрок {playerName} (ID: {clientId}) зарегистрирован.");

            // 1. Создание пакета PlayerConnected
            var msg = new PlayerConnectedMessage
            {
                PlayerId = clientId,
                SlotIndex = clientId, // Пока используем ID в качестве слота
                PlayerName = playerName
            };

            var xpacket = XPacketConverter.Serialize(XPacketType.PlayerConnected, msg);
            msg.SerializeString(xpacket); // Добавление имени

            // 2. Рассылка пакета всем клиентам
            BroadcastPacket(xpacket);

            // TODO: Здесь должна быть логика отправки списка существующих игроков новому клиенту
        }
    }
}