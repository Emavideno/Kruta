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

        private List<string> _allPlayerNames = new();

        public ObservableCollection<ICardMini> BaraholkaCards { get; } = new();
        public ObservableCollection<ICardMini> MyHandCards { get; } = new();

        private int _myPlayerIdInServer;

        [ObservableProperty]
        private int _myHealth = 20;

        [ObservableProperty]
        private bool _isMyTurn = false;

        [ObservableProperty]
        private int _myPower = 0;

        [ObservableProperty]
        private bool _isDead = false;

        // --- НОВОЕ СВОЙСТВО ПОБЕДЫ ---
        [ObservableProperty]
        private bool _isWinner = false;

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
                OnPropertyChanged();
            }
        }

        public PlayViewModel(NetworkService networkService)
        {
            _networkService = networkService;

            _networkService.OnPacketReceived -= HandlePacketWrapper;
            _networkService.OnPacketReceived += HandlePacketWrapper;

            _networkService.SendPacket(EAPacket.Create(2, 0));

            BaraholkaCards.Add(new SomeCardMini { Name = "Тест Карта", Cost = 5, CardId = 1 });

            // Хардкод для теста (сервер это перезапишет)
            IsMyTurn = true;
            MyPower = 1;
            GameStatus = "Вы ходите первым!";
        }

        private void HandlePacketWrapper(EAPacket p)
        {
            MainThread.BeginInvokeOnMainThread(() => HandlePacket(p));
        }

        private void HandlePacket(EAPacket p)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Получен пакет: Type={p.PacketType}, Subtype={p.PacketSubtype}");

            // Тип 2: Список игроков
            if (p.PacketType == 2 && p.PacketSubtype == 1)
            {
                var rawName = p.GetValueRaw(3);
                if (rawName == null) return;
                string name = Encoding.UTF8.GetString(rawName).Trim();

                if (!_allPlayerNames.Contains(name))
                {
                    _allPlayerNames.Add(name);
                }
                RebuildTable();
            }

            // Тип 3, Подтип 2: Обновление HP
            if (p.PacketType == 3 && p.PacketSubtype == 2)
            {
                if (p.HasField(3) && p.HasField(4))
                {
                    string targetName = Encoding.UTF8.GetString(p.GetValueRaw(3)).Trim();
                    int newHp = BitConverter.ToInt32(p.GetValueRaw(4), 0);

                    if (targetName == _networkService.PlayerName)
                    {
                        MyHealth = newHp;
                        if (MyHealth <= 0)
                        {
                            IsDead = true;
                            IsMyTurn = false;
                            GameStatus = "ВЫ УМЕРЛИ";
                        }
                    }
                    else
                    {
                        var opponent = Opponents.FirstOrDefault(o => o.Name == targetName);
                        if (opponent != null)
                        {
                            opponent.Health = newHp;
                        }
                    }

                    // --- ПРОВЕРКА НА ПОБЕДУ ПОСЛЕ ЛЮБОГО ИЗМЕНЕНИЯ HP ---
                    CheckWinCondition();
                }
            }

            // Тип 4, Подтип 1: Инициализация игры
            if (p.PacketType == 4 && p.PacketSubtype == 1)
            {
                if (p.HasField(5)) _myPlayerIdInServer = BitConverter.ToInt32(p.GetValueRaw(5), 0);

                if (p.HasField(6))
                {
                    var baraholkaRaw = p.GetValueRaw(6);
                    BaraholkaCards.Clear();
                    for (int i = 0; i < baraholkaRaw.Length; i += 4)
                        BaraholkaCards.Add(CreateCardById(BitConverter.ToInt32(baraholkaRaw, i)));
                }

                if (p.HasField(7))
                {
                    var handRaw = p.GetValueRaw(7);
                    MyHandCards.Clear();
                    for (int i = 0; i < handRaw.Length; i += 4)
                        MyHandCards.Add(CreateCardById(BitConverter.ToInt32(handRaw, i)));
                }

                if (p.HasField(8))
                {
                    DeckRemainingCount = BitConverter.ToInt32(p.GetValueRaw(8), 0);
                }
            }

            // Тип 5, Подтип 1: Передача хода
            if (p.PacketType == 5 && p.PacketSubtype == 1)
            {
                string activePlayerName = p.HasField(3) ? Encoding.UTF8.GetString(p.GetValueRaw(3)).Trim() : "";
                int powerValue = p.HasField(4) ? BitConverter.ToInt32(p.GetValueRaw(4), 0) : 0;

                if (activePlayerName == _networkService.PlayerName)
                {
                    IsMyTurn = !IsDead && !IsWinner; // Не даем ходить, если мертв или уже победил
                    MyPower = (IsDead || IsWinner) ? 0 : powerValue;
                }
                else
                {
                    IsMyTurn = false;
                }
            }
        }

        // --- НОВЫЙ МЕТОД ЛОГИКИ ПОБЕДЫ ---
        private void CheckWinCondition()
        {
            // Если мы уже мертвы, победить не можем
            if (IsDead) return;

            // Проверяем, есть ли вообще за столом другие подключенные игроки
            bool anyOpponentsJoined = Opponents.Any(o => o.IsConnected);

            // Считаем тех, кто еще жив среди подключенных
            int aliveOpponents = Opponents.Count(o => o.IsConnected && !o.IsDead);

            // Если кто-то был подключен, но теперь живых врагов 0 — мы победили
            if (anyOpponentsJoined && aliveOpponents == 0)
            {
                IsWinner = true;
                IsMyTurn = false;
                MyPower = 0;
                GameStatus = "ПОБЕДА!";
            }
        }

        [RelayCommand]
        private void EndTurn()
        {
            if (IsDead || IsWinner || !IsMyTurn) return;
            IsMyTurn = false;
            GameStatus = "Передача хода...";
            _networkService.SendPacket(EAPacket.Create(5, 0));
        }

        private void RebuildTable()
        {
            string myName = _networkService.PlayerName;
            if (string.IsNullOrEmpty(myName)) return;

            int myIndex = _allPlayerNames.IndexOf(myName);
            if (myIndex == -1) return;

            var opponentsToShow = new List<string>();
            for (int i = 1; i < _allPlayerNames.Count; i++)
            {
                int nextIndex = (myIndex + i) % _allPlayerNames.Count;
                opponentsToShow.Add(_allPlayerNames[nextIndex]);
            }

            foreach (var opt in Opponents)
            {
                opt.Name = "Свободно";
                opt.IsConnected = false;
                opt.Health = 20;
            }

            if (opponentsToShow.Count > 0) { Opponents[1].Name = opponentsToShow[0]; Opponents[1].IsConnected = true; }
            if (opponentsToShow.Count > 1) { Opponents[0].Name = opponentsToShow[1]; Opponents[0].IsConnected = true; }
            if (opponentsToShow.Count > 2) { Opponents[2].Name = opponentsToShow[2]; Opponents[2].IsConnected = true; }

            // После пересборки стола тоже проверим состояние
            CheckWinCondition();
        }

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
                _ => new SomeCardMini { CardId = id, Cost = 0 }
            };
        }

        [RelayCommand]
        private void AttackOpponent(string targetName)
        {
            if (IsDead || IsWinner || !IsMyTurn || MyPower <= 0) return;
            if (string.IsNullOrEmpty(targetName) || targetName == "Свободно") return;

            int dmg = MyPower;
            MyPower = 0;

            var packet = EAPacket.Create(5, 2);
            packet.SetValueRaw(3, Encoding.UTF8.GetBytes(targetName));
            _networkService.SendPacket(packet);

            GameStatus = $"Атака на {targetName} (-{dmg} HP)!";
        }
    }

    public partial class OpponentDisplay : ObservableObject
    {
        public string Position { get; set; }

        [ObservableProperty]
        private int _health = 20;

        public bool IsDead => Health <= 0;

        partial void OnHealthChanged(int value)
        {
            OnPropertyChanged(nameof(IsDead));
        }

        [ObservableProperty]
        public int maxHealth = 20;

        [ObservableProperty]
        private string _name = "Свободно";

        [ObservableProperty]
        private bool _isConnected = false;
    }
}