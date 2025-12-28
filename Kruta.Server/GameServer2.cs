using Kruta.Protocol;
using Kruta.Protocol.Serilizations;
using Kruta.Server.Handlers;
using Kruta.Server.Handlers.Interface;
using Kruta.Shared.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Kruta.Shared.Mini;
using System.IO;
using System.Threading.Tasks;

namespace Kruta.Server
{
    public class GameServer2
    {
        private TcpListener _listener = new TcpListener(IPAddress.Any, 8888);
        public List<ClientObject> Clients { get; private set; } = new List<ClientObject>();

        private bool _isRunning;
        private ServerSettings _settings;
        public bool IsGameStarted { get; set; } = false;
        public List<int> MainDeck { get; set; } = new List<int>();
        public int[] Baraholka { get; set; } = new int[5];

        public int CurrentPlayerIndex { get; private set; } = 0;

        // Словарь для отслеживания покупок за ход (Имя игрока -> Кол-во покупок)
        private Dictionary<string, int> _buysCounter = new Dictionary<string, int>();

        private readonly Dictionary<EAPacketType, IPacketHandler> _handlers;

        public GameServer2()
        {
            _settings = LoadSettings();

            EAPacketTypeManager.RegisterType(EAPacketType.PlayerConnected, 1, 0);
            EAPacketTypeManager.RegisterType(EAPacketType.PlayerDisconnected, 1, 1);
            EAPacketTypeManager.RegisterType(EAPacketType.PlayerListRequest, 2, 0);
            EAPacketTypeManager.RegisterType(EAPacketType.PlayerListUpdate, 2, 1);
            EAPacketTypeManager.RegisterType(EAPacketType.ToggleReady, 3, 0);
            EAPacketTypeManager.RegisterType(EAPacketType.PlayerReadyStatus, 3, 1);
            EAPacketTypeManager.RegisterType(EAPacketType.PlayerHpUpdate, 3, 2);
            EAPacketTypeManager.RegisterType(EAPacketType.GameStarted, 4, 0);
            EAPacketTypeManager.RegisterType(EAPacketType.GameStateUpdate, 4, 1);
            EAPacketTypeManager.RegisterType(EAPacketType.TurnAction, 5, 0);
            EAPacketTypeManager.RegisterType(EAPacketType.TurnStatus, 5, 1);
            EAPacketTypeManager.RegisterType(EAPacketType.AttackAction, 5, 2);
            EAPacketTypeManager.RegisterType(EAPacketType.PlayCardAction, 5, 3);
            EAPacketTypeManager.RegisterType(EAPacketType.BuyCardAction, 5, 4);

            _handlers = new Dictionary<EAPacketType, IPacketHandler>
            {
                { EAPacketType.PlayerConnected, new LoginHandler() },
                { EAPacketType.PlayerListRequest, new PlayerListHandler() },
                { EAPacketType.ToggleReady, new ReadyHandler() },
                { EAPacketType.TurnAction, new TurnHandler() },
                { EAPacketType.AttackAction, new TurnHandler() },
                { EAPacketType.PlayCardAction, new TurnHandler() },
                { EAPacketType.BuyCardAction, new TurnHandler() }
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
                    return JsonSerializer.Deserialize<ServerSettings>(json) ?? new ServerSettings();
                }
            }
            catch (Exception ex) { Console.WriteLine($"[CONFIG] Ошибка: {ex.Message}"); }
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
                    TcpClient tcpClient = await _listener.AcceptTcpClientAsync();
                    var clientObject = new ClientObject(tcpClient.Client, this);
                    lock (Clients) { Clients.Add(clientObject); }
                }
            }
            catch (Exception ex) { if (_isRunning) Console.WriteLine($"[SERVER] Ошибка: {ex.Message}"); }
            finally { Stop(); }
        }

        public void OnMessageReceived(ClientObject client, EAPacket packet)
        {
            try
            {
                EAPacketType type = EAPacketTypeManager.GetTypeFromPacket(packet);
                if (_handlers.TryGetValue(type, out var handler)) handler.Handle(client, packet);
            }
            catch (Exception ex) { Console.WriteLine($"[SERVER] Ошибка OnMessageReceived: {ex.Message}"); }
        }

        public void RemoveConnection(string id)
        {
            lock (Clients)
            {
                var client = Clients.FirstOrDefault(c => c.Id == id);
                if (client != null)
                {
                    Clients.Remove(client);
                    Console.WriteLine($"[SERVER] Клиент {id} отключен.");
                }
            }
        }

        public void BroadcastPlayerHp(ClientObject targetClient)
        {
            var hpPacket = EAPacket.Create(3, 2);
            hpPacket.SetValueRaw(3, Encoding.UTF8.GetBytes(targetClient.Username));
            hpPacket.SetValueRaw(4, BitConverter.GetBytes(targetClient.PlayerData.Hp));
            lock (Clients) { foreach (var client in Clients) client.Send(hpPacket); }
        }

        public async Task StartFirstTurnAsync()
        {
            Console.WriteLine("[SERVER] Подготовка к первому ходу...");
            
            // Инициализация колоды и рук игроков
            InitializeGameData();

            await Task.Delay(2000);
            lock (Clients)
            {
                if (Clients.Count > 0)
                {
                    CurrentPlayerIndex = 0;
                    foreach (var c in Clients) c.PlayerData.TurnCount = 0;
                    
                    // Обновляем состояние (показываем пустые руки и новую барахолку)
                    BroadcastGameState();
                    
                    NotifyCurrentPlayer();
                }
            }
        }

        // Логика инициализации: все карты в колоду, руки пустые
        private void InitializeGameData()
        {
            lock (Baraholka)
            {
                MainDeck.Clear();
                var rand = new Random();

                // 1. Собираем стартовые карты всех игроков в общую колоду
                foreach (var client in Clients)
                {
                    client.PlayerData.Hand.Clear(); // Сначала очищаем

                    // Добавляем стартовый набор (7 "Пшик" и 3 "Хилая палочка")
                    for (int i = 0; i < 7; i++) MainDeck.Add(3);
                    for (int i = 0; i < 3; i++) MainDeck.Add(1);
                }

                // 2. Добавляем карты магазина в колоду
                int[] shopCards = { 2, 4, 5, 6, 7, 8, 9, 10 };
                foreach (var id in shopCards)
                {
                    for (int k = 0; k < 5; k++) MainDeck.Add(id);
                }

                // 3. Перемешиваем колоду
                MainDeck = MainDeck.OrderBy(x => Guid.NewGuid()).ToList();

                // --- НОВАЯ ЛОГИКА: Раздаем по 3 карты каждому игроку ---
                foreach (var client in Clients)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (MainDeck.Count > 0)
                        {
                            int cardId = MainDeck[0];
                            client.PlayerData.Hand.Add(new ServerCard { Id = cardId });
                            MainDeck.RemoveAt(0);
                        }
                    }
                }

                // 4. Заполняем Барахолку из оставшейся колоды
                for (int i = 0; i < Baraholka.Length; i++)
                {
                    if (MainDeck.Count > 0)
                    {
                        Baraholka[i] = MainDeck[0];
                        MainDeck.RemoveAt(0);
                    }
                    else
                    {
                        Baraholka[i] = 0;
                    }
                }
            }
        }

        public void NotifyCurrentPlayer()
        {
            lock (Clients)
            {
                if (Clients.Count == 0) return;
                var activeClient = Clients[CurrentPlayerIndex];
                activeClient.PlayerData.TurnCount++;
                activeClient.PlayerData.Power = activeClient.PlayerData.TurnCount;

                // СБРОС СЧЕТЧИКА ПОКУПОК НА НОВЫЙ ХОД
                if (!_buysCounter.ContainsKey(activeClient.Username))
                    _buysCounter[activeClient.Username] = 0;
                else
                    _buysCounter[activeClient.Username] = 0;

                var turnPacket = EAPacket.Create(5, 1);
                turnPacket.SetValueRaw(3, Encoding.UTF8.GetBytes(activeClient.Username));
                turnPacket.SetValueRaw(4, BitConverter.GetBytes(activeClient.PlayerData.Power));
                foreach (var c in Clients) c.Send(turnPacket);
            }
        }

        public void NextTurn()
        {
            lock (Clients)
            {
                if (Clients.Count == 0) return;
                int startingIndex = CurrentPlayerIndex;
                bool foundAlive = false;
                while (!foundAlive)
                {
                    CurrentPlayerIndex = (CurrentPlayerIndex + 1) % Clients.Count;
                    if (Clients[CurrentPlayerIndex].PlayerData.Hp > 0) foundAlive = true;
                    if (CurrentPlayerIndex == startingIndex && !foundAlive) return;
                }
                NotifyCurrentPlayer();
            }
        }

        public void ProcessAttack(ClientObject attacker, string targetName)
        {
            lock (Clients)
            {
                var target = Clients.FirstOrDefault(c => c.Username.Trim().Equals(targetName.Trim(), StringComparison.OrdinalIgnoreCase));
                if (target == null || attacker.PlayerData.Power <= 0) return;

                target.PlayerData.Hp -= attacker.PlayerData.Power;
                attacker.PlayerData.Power = 0;
                BroadcastPlayerHp(target);

                var powerUpdate = EAPacket.Create(5, 1);
                powerUpdate.SetValueRaw(3, Encoding.UTF8.GetBytes(attacker.Username));
                powerUpdate.SetValueRaw(4, BitConverter.GetBytes(0));
                attacker.Send(powerUpdate);
            }
        }

        public void ProcessPlayCard(ClientObject player, int cardId)
        {
            string cardName = GetCardNameById(cardId);
            string message = $"Карта \"{cardName}\" разыграна игроком {player.Username}!";
            var notifyPacket = EAPacket.Create(5, 3);
            notifyPacket.SetValueRaw(4, Encoding.UTF8.GetBytes(message));
            lock (Clients) { foreach (var c in Clients) c.Send(notifyPacket); }
        }

        public void ProcessBuyCard(ClientObject player, int cardId)
        {
            // ПРОВЕРКА ЛИМИТА ПОКУПОК (МАКСИМУМ 2)
            if (_buysCounter.TryGetValue(player.Username, out int count) && count >= 2)
            {
                // Отправляем сообщение об ошибке только этому игроку
                var errorPacket = EAPacket.Create(5, 3);
                string msg = "Лимит покупок: макс 2 карты за ход!";
                errorPacket.SetValueRaw(4, Encoding.UTF8.GetBytes(msg));
                player.Send(errorPacket);
                return;
            }

            lock (Baraholka)
            {
                int index = Array.IndexOf(Baraholka, cardId);
                if (index == -1) return;

                // 1. Добавляем карту в список руки на сервере
                player.PlayerData.Hand.Add(new ServerCard { Id = cardId });

                // 2. Увеличиваем счетчик покупок
                _buysCounter[player.Username]++;

                // 3. Обновляем Барахолку
                string cardName = GetCardNameById(cardId);
                int nextCardId = 0;
                if (MainDeck.Count > 0)
                {
                    nextCardId = MainDeck[0];
                    MainDeck.RemoveAt(0);
                }
                Baraholka[index] = nextCardId;

                // 4. Уведомляем логом (можно добавить инфо, какая это по счету покупка)
                string message = $"Карту \"{cardName}\" купил игрок {player.Username} ({_buysCounter[player.Username]}/2)";
                var notifyPacket = EAPacket.Create(5, 3);
                notifyPacket.SetValueRaw(4, Encoding.UTF8.GetBytes(message));

                lock (Clients)
                {
                    foreach (var c in Clients) c.Send(notifyPacket);
                }

                // 5. Рассылаем состояние игры
                BroadcastGameState();
            }
        }

        public void BroadcastGameState()
        {
            lock (Clients)
            {
                foreach (var client in Clients)
                {
                    var p = EAPacket.Create(4, 1);

                    // Поле 6: Барахолка (общая для всех)
                    byte[] baraholkaBytes = new byte[Baraholka.Length * 4];
                    Buffer.BlockCopy(Baraholka, 0, baraholkaBytes, 0, baraholkaBytes.Length);
                    p.SetValueRaw(6, baraholkaBytes);

                    // Поле 7: Карты в руке КОНКРЕТНОГО клиента
                    var handIds = client.PlayerData.Hand
                        .OfType<ServerCard>()
                        .Select(c => c.Id)
                        .Where(id => id > 0)
                        .ToArray();

                    byte[] handBytes = new byte[handIds.Length * 4];
                    Buffer.BlockCopy(handIds, 0, handBytes, 0, handBytes.Length);
                    p.SetValueRaw(7, handBytes);

                    // Поле 8: Размер колоды
                    p.SetValueRaw(8, BitConverter.GetBytes(MainDeck.Count));

                    // Отправляем пакет конкретному клиенту
                    client.Send(p);
                }
            }
        }

        private string GetCardNameById(int id) => id switch
        {
            1 => "Хилая палочка",
            2 => "Боевой саксофон",
            3 => "Пшик",
            4 => "Сопливый рыцарь",
            5 => "Близнецы",
            6 => "Волшебная палочка",
            7 => "Знак",
            8 => "Дикая магия",
            9 => "Инферно",
            10 => "Крутагидон",
            _ => "Неизвестная карта"
        };

        public void Stop()
        {
            _isRunning = false;
            _listener?.Stop();
            lock (Clients)
            {
                foreach (var c in Clients) c.Close();
                Clients.Clear();
            }
        }
    }

    public class ServerCard : Kruta.Shared.Models.Interfaces.ICard
    {
        public int Id { get; set; }
    }
}