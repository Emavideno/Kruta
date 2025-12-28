using Kruta.Protocol;
using Kruta.Protocol.Serilizations;
using Kruta.Server.Handlers;
using Kruta.Server.Handlers.Interface;
using Kruta.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Kruta.Shared.Mini;

namespace Kruta.Server
{
    public class GameServer2
    {
        private TcpListener _listener = new TcpListener(IPAddress.Any, 8888); // сервер для прослушивания
        public List<ClientObject> Clients { get; private set; } = new List<ClientObject>(); // все подключения, список пользователей

        private bool _isRunning;
        private ServerSettings _settings;
        public bool IsGameStarted { get; set; } = false;
        public List<int> MainDeck { get; set; } = new List<int>();
        public int[] Baraholka { get; set; } = new int[5];

        public int CurrentPlayerIndex { get; private set; } = 0;

        // Словарь: Тип пакета -> Обработчик
        private readonly Dictionary<EAPacketType, IPacketHandler> _handlers;

        public GameServer2()
        {
            _settings = LoadSettings();

            // Существующие типы
            EAPacketTypeManager.RegisterType(EAPacketType.PlayerConnected, 1, 0);
            EAPacketTypeManager.RegisterType(EAPacketType.PlayerDisconnected, 1, 1);
            EAPacketTypeManager.RegisterType(EAPacketType.PlayerListRequest, 2, 0);
            EAPacketTypeManager.RegisterType(EAPacketType.PlayerListUpdate, 2, 1);

            // НОВЫЕ ТИПЫ
            EAPacketTypeManager.RegisterType(EAPacketType.ToggleReady, 3, 0);
            EAPacketTypeManager.RegisterType(EAPacketType.PlayerReadyStatus, 3, 1);
            EAPacketTypeManager.RegisterType(EAPacketType.PlayerHpUpdate, 3, 2);
            EAPacketTypeManager.RegisterType(EAPacketType.GameStarted, 4, 0);
            EAPacketTypeManager.RegisterType(EAPacketType.GameStateUpdate, 4, 1);

            EAPacketTypeManager.RegisterType(EAPacketType.TurnAction, 5, 0);
            EAPacketTypeManager.RegisterType(EAPacketType.TurnStatus, 5, 1);

            _handlers = new Dictionary<EAPacketType, IPacketHandler>
            {
                { EAPacketType.PlayerConnected, new LoginHandler() },
                { EAPacketType.PlayerListRequest, new PlayerListHandler() },
                { EAPacketType.ToggleReady, new ReadyHandler() },
                { EAPacketType.TurnAction, new TurnHandler() }

                };
            }

        private ServerSettings LoadSettings()
        {
            string path = "settingsForServer.json";
            try
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    // Если файл пустой или содержит мусор, Deserialize выбросит исключение
                    return JsonSerializer.Deserialize<ServerSettings>(json) ?? new ServerSettings();
                }
            }
            catch (JsonException jex)
            {
                Console.WriteLine($"[CONFIG] Файл настроек поврежден (ошибка JSON): {jex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONFIG] Ошибка чтения файла: {ex.Message}");
            }
            return new ServerSettings();
        }

        public async Task StartAsync()
        {
            IPAddress ip = IPAddress.Parse(_settings.Host);

            _listener = new TcpListener(ip, _settings.Port);
            _listener.Start();

            _isRunning = true;
            Console.WriteLine($"[SERVER] Запущен на {ip}:{_settings.Port}");


            try
            {
                while (_isRunning)
                {
                    // Асинхронно ждем клиента
                    TcpClient tcpClient = await _listener.AcceptTcpClientAsync();

                    Socket clientSocket = tcpClient.Client;

                    // Создаем объект игрока. Передаем ему сокет и ссылку на текущий сервер (this).
                    var clientObject = new ClientObject(clientSocket, this);

                    lock (Clients) // Запрещаем другим потокам трогать список игроков в эту миллисекунду.
                    {
                        Clients.Add(clientObject); // Добавляем новичка в "комнату".
                    }
                }
            }
            catch (Exception ex)
            {
                if (_isRunning) Console.WriteLine($"[SERVER] Ошибка: {ex.Message}");
            }
            finally
            {
                Stop();
            }

        }


        //Приемка сообщения от клиента
        public void OnMessageReceived(ClientObject client, EAPacket packet)
        {
            try
            {
                //узнаем, что за событие пришло
                EAPacketType type = EAPacketTypeManager.GetTypeFromPacket(packet);
                Console.WriteLine($"[SERVER] Получен пакет от клиента {client.Id}. Type={packet.PacketType} Subtype={packet.PacketSubtype} MappedType={type}");

                // Ищем обработчик в словаре для вызова логики в зависимости от типа пакета
                if (_handlers.TryGetValue(type, out var handler))
                {
                    handler.Handle(client, packet);
                }
                else
                {
                    Console.WriteLine($"[WARN] Нет обработчика для типа: {type}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SERVER] Исключение в OnMessageReceived: {ex}");
            }
        }


        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();

            lock (Clients)
            {
                foreach (var client in Clients) client.Close();
                Clients.Clear();
            }
            Console.WriteLine("[SERVER] Остановлен.");
        }



        //отрубка игрока 
        public void RemoveConnection(string id)
        {
            lock (Clients) // Снова блокируем список для безопасности (потокобезопасность).
            {
                // Ищем в списке клиента, у которого Id совпадает с тем, что нам передали.
                var client = Clients.FirstOrDefault(c => c.Id == id);

                if (client != null)
                {
                    Clients.Remove(client); // Удаляем его. Теперь сервер о нем не знает.
                    Console.WriteLine($"[SERVER] Клиент {id} отключен.");
                }
            }
        }

        //Изменение хп игрока
        public void BroadcastPlayerHp(ClientObject targetClient)
        {
            // Создаем пакет (Type 3, Subtype 2)
            var hpPacket = EAPacket.Create(3, 2);

            // Поле 3: Ник или ID игрока (чтобы клиенты поняли, чье HP обновилось)
            hpPacket.SetValueRaw(3, Encoding.UTF8.GetBytes(targetClient.Username));

            // Поле 4: Новое значение HP (переводим int в byte[])
            hpPacket.SetValueRaw(4, BitConverter.GetBytes(targetClient.PlayerData.Hp));

            lock (Clients)
            {
                foreach (var client in Clients)
                {
                    client.Send(hpPacket);
                }
            }
        }

        // В GameServer2.cs метод уже почти правильный, убедитесь, что он такой:
        // В GameServer2.cs
        public async Task StartFirstTurnAsync()
        {
            Console.WriteLine("[SERVER] Ожидание прогрузки клиентов (3 сек)...");
            await Task.Delay(3000); // Время, чтобы смартфоны успели открыть страницу

            lock (Clients)
            {
                if (Clients.Count > 0)
                {
                    // Устанавливаем индекс на первого игрока
                    CurrentPlayerIndex = 0;

                    var firstPlayer = Clients[0];

                    // Создаем пакет "Твой ход"
                    var turnPacket = EAPacket.Create(5, 1);

                    // Отправляем конкретно первому подключившемуся
                    firstPlayer.Send(turnPacket);

                    Console.WriteLine($"[SERVER] Игра началась! Первый ход передан: {firstPlayer.Username}");
                }
                else
                {
                    Console.WriteLine("[SERVER] Ошибка: Нет игроков для начала хода.");
                }
            }
        }

        // Метод уведомления игрока, что сейчас его ход
        private void NotifyCurrentPlayer()
        {
            if (Clients.Count == 0) return;

            var activeClient = Clients[CurrentPlayerIndex];

            // Создаем пакет хода
            var turnPacket = EAPacket.Create(5, 1);
            // Добавим в пакет ID того, кто сейчас должен ходить (например, в поле 3)
            turnPacket.SetValueRaw(3, Encoding.UTF8.GetBytes(activeClient.Username));

            // Рассылаем ВСЕМ. Каждый клиент сам проверит: "Это мой ник? Если да - включаю кнопку"
            lock (Clients)
            {
                foreach (var c in Clients)
                {
                    c.Send(turnPacket);
                }
            }
        }

        // Метод переключения хода
        public void NextTurn()
        {
            lock (Clients)
            {
                CurrentPlayerIndex++;
                if (CurrentPlayerIndex >= Clients.Count) CurrentPlayerIndex = 0;
                NotifyCurrentPlayer();
            }
        }
    }
}
