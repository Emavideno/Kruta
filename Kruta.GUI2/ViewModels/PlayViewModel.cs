using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kruta.GUI2.Services;
using Kruta.Protocol;
using Kruta.Shared.Mini;
using Kruta.Shared.Mini.Cards;
using System.Collections.ObjectModel;
using System.Text;

namespace Kruta.GUI2.ViewModels
{
    public partial class PlayViewModel : ObservableObject
    {
        private readonly NetworkService _networkService;

        [ObservableProperty]
        private string _gameStatus = "Ожидание игроков...";

        // Временный список всех имен, полученных от сервера
        private List<string> _allPlayerNames = new();

        // Коллекция для центра стола (Барахолка)
        public ObservableCollection<ICardMini> BaraholkaCards { get; } = new();

        // Коллекция для нижней панели (Ваши карты)
        public ObservableCollection<ICardMini> MyHandCards { get; } = new();

        // Свойство для хранения вашего ID (полученного от сервера)
        private int _myPlayerIdInServer;

        [ObservableProperty]
        private int _myHealth = 20;

        [ObservableProperty]
        private bool _isMyTurn = false;

        [ObservableProperty]
        private int _myPower = 0;

        public ObservableCollection<OpponentDisplay> Opponents { get; } = new()
        {
            new OpponentDisplay { Position = "Top" },
            new OpponentDisplay { Position = "Left" },
            new OpponentDisplay { Position = "Right" }
        };

        private int _deckRemainingCount;
        public int DeckRemainingCount
        {
            get => _deckRemainingCount;
            set
            {
                _deckRemainingCount = value;
                OnPropertyChanged(); // Обязательно для обновления UI
            }
        }

        public PlayViewModel(NetworkService networkService)
        {
            _networkService = networkService;

            // Гарантируем, что подписка только одна
            _networkService.OnPacketReceived -= HandlePacketWrapper;
            _networkService.OnPacketReceived += HandlePacketWrapper;

            // Запрашиваем актуальный список игроков при входе
            _networkService.SendPacket(EAPacket.Create(2, 0));

            // Тестовые карты в Барахолке (удалить позже)
            BaraholkaCards.Add(new SomeCardMini { Name = "Тест Карта", Cost = 5, CardId = 1 });

            // --- ХАРДКОД СТАРТА ДЛЯ ПЕРВОГО ИГРОКА ---
            // Если список имен пуст или мы там первые - даем себе мощь авансом
            // Это перекроется сервером, когда придет пакет 5:1
            IsMyTurn = true;
            MyPower = 1;
            GameStatus = "Вы ходите первым!";
        }

        // Вынесите логику в обертку
        private void HandlePacketWrapper(EAPacket p)
        {
            MainThread.BeginInvokeOnMainThread(() => HandlePacket(p));
        }

        private void HandlePacket(EAPacket p)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Получен пакет: Type={p.PacketType}, Subtype={p.PacketSubtype}");

            // Тип 2, Подтип 1: Список игроков (или новый игрок)
            if (p.PacketType == 2 && p.PacketSubtype == 1)
            {
                var rawName = p.GetValueRaw(3);
                if (rawName == null) return;
                string name = Encoding.UTF8.GetString(rawName).Trim();

                // Добавляем в общий список всех, кого прислал сервер
                if (!_allPlayerNames.Contains(name))
                {
                    _allPlayerNames.Add(name);
                }

                // Пересчитываем положение игроков на столе
                RebuildTable();
            }

            // ОБРАБОТКА ОБНОВЛЕНИЯ ХП (Тип 3, Подтип 2)
            if (p.PacketType == 3 && p.PacketSubtype == 2)
            {
                if (p.HasField(3) && p.HasField(4))
                {
                    string targetName = Encoding.UTF8.GetString(p.GetValueRaw(3)).Trim();
                    int newHp = BitConverter.ToInt32(p.GetValueRaw(4), 0);

                    // ЛОГ 1: Получение пакета
                    System.Diagnostics.Debug.WriteLine($"[HP_DEBUG] СЕРВЕР ПРИСЛАЛ ОБНОВЛЕНИЕ: Игрок={targetName}, Новое HP={newHp}");

                    MainThread.BeginInvokeOnMainThread(() => {
                        if (targetName == _networkService.PlayerName)
                        {
                            System.Diagnostics.Debug.WriteLine($"[HP_DEBUG] Обновляю СВОЕ здоровье: {MyHealth} -> {newHp}");
                            MyHealth = newHp;
                        }
                        else
                        {
                            var opponent = Opponents.FirstOrDefault(o => o.Name == targetName);
                            if (opponent != null)
                            {
                                System.Diagnostics.Debug.WriteLine($"[HP_DEBUG] Обновляю здоровье ОППОНЕНТА {targetName}: {opponent.Health} -> {newHp}");
                                opponent.Health = newHp;
                            }
                            else
                            {
                                // ЛОГ 2: Если не нашли оппонента в списке UI
                                System.Diagnostics.Debug.WriteLine($"[HP_DEBUG] ОШИБКА: Игрок {targetName} не найден в списке Opponents!");
                                foreach (var opt in Opponents) System.Diagnostics.Debug.WriteLine($"[HP_DEBUG] В списке есть: '{opt.Name}'");
                            }
                        }
                    });
                }
            }

            // Тип 4, Подтип 1: Данные инициализации игры (Mini модели)
            if (p.PacketType == 4 && p.PacketSubtype == 1)
            {
                // 1. Мой ID (читаем только если есть)
                if (p.HasField(5))
                {
                    var idRaw = p.GetValueRaw(5);
                    _myPlayerIdInServer = BitConverter.ToInt32(idRaw, 0);
                }

                // 2. Барахолка
                if (p.HasField(6))
                {
                    var baraholkaRaw = p.GetValueRaw(6);
                    BaraholkaCards.Clear();
                    for (int i = 0; i < baraholkaRaw.Length; i += 4)
                    {
                        int id = BitConverter.ToInt32(baraholkaRaw, i);
                        BaraholkaCards.Add(CreateCardById(id));
                    }
                }

                // 3. Стартовые карты
                if (p.HasField(7))
                {
                    var handRaw = p.GetValueRaw(7);
                    MyHandCards.Clear();
                    for (int i = 0; i < handRaw.Length; i += 4)
                    {
                        int id = BitConverter.ToInt32(handRaw, i);
                        MyHandCards.Add(CreateCardById(id));
                    }
                }

                // 4. Количество карт (то самое поле 8)
                if (p.HasField(8))
                {
                    var deckRaw = p.GetValueRaw(8);
                    int newCount = BitConverter.ToInt32(deckRaw, 0);
                    System.Diagnostics.Debug.WriteLine($"[NETWORK] Обновлено кол-во карт: {newCount}");
                    DeckRemainingCount = newCount;
                }

                // ХАРДКОД ДЛЯ ПЕРВОГО ИГРОКА ПРИ СТАРТЕ
                if (_allPlayerNames.Count > 0 && _allPlayerNames[0] == _networkService.PlayerName)
                {
                    IsMyTurn = true;
                    MyPower = 1; // Устанавливаем 1 принудительно
                    GameStatus = "Ваш ход! (Мощь: 1)";
                }
                else
                {
                    IsMyTurn = false;
                    MyPower = 0;
                    GameStatus = "Ожидание хода первого игрока...";
                }
            }


            // Type 5: Turn, Subtype 1: Server объявляет текущего игрока
            // В методе HandlePacket обновите обработку Типа 5 Подтипа 1
            if (p.PacketType == 5 && p.PacketSubtype == 1)
            {
                string activePlayerName = "";
                int powerValue = 0;

                if (p.HasField(3))
                    activePlayerName = Encoding.UTF8.GetString(p.GetValueRaw(3)).Trim();

                if (p.HasField(4))
                    powerValue = BitConverter.ToInt32(p.GetValueRaw(4), 0);

                // ВАЖНЫЙ ЛОГ ДЛЯ КЛИЕНТА
                System.Diagnostics.Debug.WriteLine($"[CLIENT RECEIVE] Ход игрока: {activePlayerName}, Пришедшая мощь: {powerValue}");

                MainThread.BeginInvokeOnMainThread(() => {
                    if (activePlayerName == _networkService.PlayerName)
                    {
                        IsMyTurn = true;
                        MyPower = powerValue; // Сюда должна прийти 1 от сервера в самом начале
                    }
                    else
                    {
                        IsMyTurn = false;
                    }
                });
            }


        }

        // Команда кнопки (у тебя она уже почти правильная, проверяем)
        [RelayCommand]
        private void EndTurn()
        {
            if (!IsMyTurn) return; // Защита на клиенте

            // Визуальный отклик мгновенно
            IsMyTurn = false;
            GameStatus = "Передача хода...";

            // Отправляем на сервер: Type 5, Subtype 0 (Я всё)
            var packet = EAPacket.Create(5, 0);
            _networkService.SendPacket(packet);
        }

        private void RebuildTable()
        {
            string myName = _networkService.PlayerName;
            if (string.IsNullOrEmpty(myName)) return;

            int myIndex = _allPlayerNames.IndexOf(myName);
            if (myIndex == -1) return;

            // 1. Получаем список тех, кто сидит за столом кроме нас (по порядку хода)
            var opponentsToShow = new List<string>();
            for (int i = 1; i < _allPlayerNames.Count; i++)
            {
                int nextIndex = (myIndex + i) % _allPlayerNames.Count;
                opponentsToShow.Add(_allPlayerNames[nextIndex]);
            }

            // 2. Сброс всех слотов
            foreach (var opt in Opponents)
            {
                opt.Name = "Свободно";
                opt.IsConnected = false;
            }

            // 3. Распределяем по позициям:
            // Первый после нас (i=0) -> СЛЕВА (Opponents[1])
            if (opponentsToShow.Count > 0)
            {
                Opponents[1].Name = opponentsToShow[0];
                Opponents[1].IsConnected = true;
            }

            // Второй после нас (i=1) -> НАПРОТИВ (Opponents[0])
            if (opponentsToShow.Count > 1)
            {
                Opponents[0].Name = opponentsToShow[1];
                Opponents[0].IsConnected = true;
            }

            // Третий после нас (i=2) -> СПРАВА (Opponents[2])
            if (opponentsToShow.Count > 2)
            {
                Opponents[2].Name = opponentsToShow[2];
                Opponents[2].IsConnected = true;
            }
        }

        // Вспомогательный метод для создания объекта карты по ID
        private ICardMini CreateCardById(int id)
        {
            return id switch
            {
                1 => new LimpWandCard(),
                2 => new BattleSaxCard(),
                3 => new FizzleCard(),
                4 => new SnotKnightCard(),
                5 => new TwinsCard(),
                6 => new WandCard(),
                7 => new SignCard(),
                8 => new WildMagicCard(),
                9 => new InfernoCard(),
                10 => new KrutagidonCard(),
                _ => new SomeCardMini { CardId = id, Cost = 0 } // Заглушка для неизвестных ID
            };
        }

        // Команда для атаки конкретного противника
        [RelayCommand]
        private void AttackOpponent(string targetName)
        {
            System.Diagnostics.Debug.WriteLine($"[ATTACK_DEBUG] Попытка атаки на {targetName}. Моя мощь: {MyPower}");
            if (!IsMyTurn) { System.Diagnostics.Debug.WriteLine("[ATTACK_DEBUG] Отмена: Не мой ход!"); return; }

            // Проверки на клиенте
            if (!IsMyTurn) return;
            if (MyPower <= 0) return;
            if (string.IsNullOrEmpty(targetName) || targetName == "Свободно") return;

            // Оптимистичное обновление UI (сразу ставим 0, чтобы кнопка исчезла)
            // Реальное подтверждение придет от сервера через мс
            int dmg = MyPower;
            MyPower = 0;

            // Формируем пакет 5:2
            var packet = EAPacket.Create(5, 2);
            packet.SetValueRaw(3, Encoding.UTF8.GetBytes(targetName));

            _networkService.SendPacket(packet);

            GameStatus = $"Атака на {targetName} (-{dmg} HP)!";

            System.Diagnostics.Debug.WriteLine($"[ATTACK_DEBUG] Пакет 5:2 отправлен на сервер для цели {targetName}");
            _networkService.SendPacket(packet);
        }


    }

    public partial class OpponentDisplay : ObservableObject
    {
        public string Position { get; set; }

        [ObservableProperty]
        private int _health = 20; // Начальное значение


        [ObservableProperty]
        public int maxHealth = 20;

        [ObservableProperty]
        private string _name = "Свободно";

        [ObservableProperty]
        private bool _isConnected = false;
    }
}