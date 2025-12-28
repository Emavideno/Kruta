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
            EAPacketTypeManager.RegisterType(EAPacketType.AttackAction, 5, 2);

            _handlers = new Dictionary<EAPacketType, IPacketHandler>
            {
                { EAPacketType.PlayerConnected, new LoginHandler() },
                { EAPacketType.PlayerListRequest, new PlayerListHandler() },
                { EAPacketType.ToggleReady, new ReadyHandler() },
                { EAPacketType.TurnAction, new TurnHandler() },
                { EAPacketType.AttackAction, new TurnHandler() }

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
            var hpPacket = EAPacket.Create(3, 2);
            hpPacket.SetValueRaw(3, Encoding.UTF8.GetBytes(targetClient.Username));
            hpPacket.SetValueRaw(4, BitConverter.GetBytes(targetClient.PlayerData.Hp));

            Console.WriteLine($"[SERVER_HP] Рассылка HP: {targetClient.Username} теперь имеет {targetClient.PlayerData.Hp} HP");

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
            Console.WriteLine("[SERVER] Подготовка к первому ходу...");
            await Task.Delay(2000);

            lock (Clients)
            {
                if (Clients.Count > 0)
                {
                    CurrentPlayerIndex = 0;

                    // 1. Сбрасываем всем в 0
                    foreach (var c in Clients) c.PlayerData.TurnCount = 0;

                    // --- ВОТ ЭТА СТРОКА РЕШАЕТ ПРОБЛЕМУ ---
                    // Говорим серверу, что ПЕРВЫЙ игрок УЖЕ находится на своем 1-м ходу
                    Clients[0].PlayerData.TurnCount = 1;
                    // --------------------------------------

                    // Теперь, когда ты нажмешь "Конец хода", сервер переключит на второго,
                    // а когда вернется к тебе, он прибавит к этой единице еще одну и пришлет 2.

                    NotifyCurrentPlayer(); // Этот метод разошлет пакет 5:1 (Игрок 1, Мощь 1)

                    Console.WriteLine($"[SERVER] Игра официально стартовала. Ходит: {Clients[0].Username}");
                }
            }
        }

        // Метод уведомления ВСЕХ о том, чей сейчас ход
        private void NotifyCurrentPlayer()
        {
            lock (Clients)
            {
                if (Clients.Count == 0) return;

                var activeClient = Clients[CurrentPlayerIndex];

                // ЛОГИКА МОЩИ
                int oldTurnCount = activeClient.PlayerData.TurnCount;
                activeClient.PlayerData.TurnCount++; // Увеличиваем счетчик ходов
                activeClient.PlayerData.Power = activeClient.PlayerData.TurnCount;

                Console.WriteLine($"[POWER LOG] Игрок: {activeClient.Username}");
                Console.WriteLine($"[POWER LOG] Старый TurnCount: {oldTurnCount}");
                Console.WriteLine($"[POWER LOG] НОВЫЙ TurnCount: {activeClient.PlayerData.TurnCount}");
                Console.WriteLine($"[POWER LOG] Отправляемая Мощь (Power): {activeClient.PlayerData.Power}");

                var turnPacket = EAPacket.Create(5, 1);
                turnPacket.SetValueRaw(3, Encoding.UTF8.GetBytes(activeClient.Username));
                turnPacket.SetValueRaw(4, BitConverter.GetBytes(activeClient.PlayerData.Power));

                foreach (var c in Clients)
                {
                    c.Send(turnPacket);
                }
            }
        }

        // Публичный метод для переключения на следующего
        public void NextTurn()
        {
            lock (Clients)
            {
                if (Clients.Count == 0) return;

                Console.WriteLine("\n=== [NEXT TURN START] ===");
                Console.WriteLine($"[LOG] Игрок {Clients[CurrentPlayerIndex].Username} (индекс {CurrentPlayerIndex}) закончил ход.");

                // Увеличиваем индекс
                CurrentPlayerIndex++;

                // Если вышли за пределы списка, возвращаемся к началу (цикл)
                if (CurrentPlayerIndex >= Clients.Count)
                {
                    Console.WriteLine("[LOG] Достигнут конец списка игроков. Переход на КРУГ 2 (Индекс -> 0)");
                    CurrentPlayerIndex = 0;
                }

                Console.WriteLine($"[LOG] Следующий по очереди: {Clients[CurrentPlayerIndex].Username} (индекс {CurrentPlayerIndex})");

                // Уведомляем всех
                NotifyCurrentPlayer();
                Console.WriteLine("=== [NEXT TURN END] ===\n");
            }
        }

        public void ProcessAttack(ClientObject attacker, string targetName)
        {
            lock (Clients)
            {
                Console.WriteLine($"\n--- [LOG ATTACK START] ---");
                Console.WriteLine($"[DEBUG] Игрок {attacker.Username} бьет цель: '{targetName}'");

                // 1. Проверка очереди
                if (Clients.Count <= CurrentPlayerIndex || Clients[CurrentPlayerIndex].Id != attacker.Id)
                {
                    Console.WriteLine($"[CHEATER] Ошибка: {attacker.Username} пытался атаковать вне очереди!");
                    return;
                }

                // 2. Проверка наличия мощи
                if (attacker.PlayerData.Power <= 0)
                {
                    Console.WriteLine($"[GAME] У {attacker.Username} недостаточно мощи (Текущая: {attacker.PlayerData.Power})");
                    return;
                }

                // 3. Поиск цели
                var target = Clients.FirstOrDefault(c => c.Username.Trim().Equals(targetName.Trim(), StringComparison.OrdinalIgnoreCase));

                if (target == null)
                {
                    Console.WriteLine($"[ERROR] Цель '{targetName}' не найдена!");
                    Console.WriteLine("[DEBUG] Список доступных имен на сервере:");
                    foreach (var c in Clients)
                    {
                        Console.WriteLine($"  - '{c.Username}' (ID: {c.Id})");
                    }
                    return;
                }

                // 4. Расчет урона
                int damage = attacker.PlayerData.Power;
                int oldHp = target.PlayerData.Hp;

                // Применение
                attacker.PlayerData.Power = 0;
                target.PlayerData.Hp -= damage;

                Console.WriteLine($"[SUCCESS] Попадание! {target.Username}: {oldHp} HP -> {target.PlayerData.Hp} HP. Урон: {damage}");

                // 5. Рассылка HP жертвы всем
                BroadcastPlayerHp(target);

                // 6. Обнуление мощи у атакующего (отправляем ему пакет 5:1)
                var powerUpdatePacket = EAPacket.Create(5, 1);
                powerUpdatePacket.SetValueRaw(3, Encoding.UTF8.GetBytes(attacker.Username));
                powerUpdatePacket.SetValueRaw(4, BitConverter.GetBytes(0));

                attacker.Send(powerUpdatePacket);
                Console.WriteLine($"[DEBUG] Мощь атакующего {attacker.Username} сброшена в 0.");
                Console.WriteLine($"--- [LOG ATTACK END] ---\n");
            }
        }
    }
}
