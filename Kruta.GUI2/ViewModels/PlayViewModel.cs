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
        private CancellationTokenSource? _statusTimerTokenSource;

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

        [ObservableProperty]
        private bool _isWinner = false;

        [ObservableProperty]
        private bool _hasPlayedCardThisTurn = false;

        private string _currentTurnHolderName = "";

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
        }

        private void HandlePacketWrapper(EAPacket p)
        {
            MainThread.BeginInvokeOnMainThread(() => HandlePacket(p));
        }

        private void HandlePacket(EAPacket p)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Получен пакет: Type={p.PacketType}, Subtype={p.PacketSubtype}");

            // Тип 2, Подтип 1: Список игроков
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
                    CheckWinCondition();
                }
            }

            // Тип 4, Подтип 1: Обновление состояния игры (GameStateUpdate)
            if (p.PacketType == 4 && p.PacketSubtype == 1)
            {
                if (p.HasField(5)) _myPlayerIdInServer = BitConverter.ToInt32(p.GetValueRaw(5), 0);

                // Обновление Барахолки
                if (p.HasField(6))
                {
                    var baraholkaRaw = p.GetValueRaw(6);
                    BaraholkaCards.Clear();
                    for (int i = 0; i < baraholkaRaw.Length; i += 4)
                    {
                        int id = BitConverter.ToInt32(baraholkaRaw, i);
                        if (id > 0) BaraholkaCards.Add(CreateCardById(id));
                    }
                }

                // --- ИСПРАВЛЕННОЕ ОБНОВЛЕНИЕ РУКИ ---
                if (p.HasField(7))
                {
                    var handRaw = p.GetValueRaw(7);
                    var serverCardIds = new List<int>();
                    for (int i = 0; i < handRaw.Length; i += 4)
                    {
                        int cardId = BitConverter.ToInt32(handRaw, i);
                        if (cardId > 0) serverCardIds.Add(cardId);
                    }

                    // Полная синхронизация: очищаем и добавляем всё, что прислал сервер
                    // Это гарантирует, что купленная карта появится, а старые не пропадут
                    MyHandCards.Clear();
                    foreach (var id in serverCardIds)
                    {
                        MyHandCards.Add(CreateCardById(id));
                    }
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Рука синхронизирована. Карт: {MyHandCards.Count}");
                }

                if (p.HasField(8))
                {
                    DeckRemainingCount = BitConverter.ToInt32(p.GetValueRaw(8), 0);
                }
            }

            // Тип 5, Подтип 1: Передача хода
            if (p.PacketType == 5 && p.PacketSubtype == 1)
            {
                _currentTurnHolderName = p.HasField(3) ? Encoding.UTF8.GetString(p.GetValueRaw(3)).Trim() : "";
                int powerValue = p.HasField(4) ? BitConverter.ToInt32(p.GetValueRaw(4), 0) : 0;

                UpdateDefaultStatus();

                if (_currentTurnHolderName == _networkService.PlayerName)
                {
                    IsMyTurn = !IsDead && !IsWinner;
                    MyPower = (IsDead || IsWinner) ? 0 : powerValue;
                    HasPlayedCardThisTurn = false;
                }
                else
                {
                    IsMyTurn = false;
                }
            }

            // Тип 5, Подтип 3: Сообщения от сервера (Лог боя)
            if (p.PacketType == 5 && p.PacketSubtype == 3)
            {
                if (p.HasField(4))
                {
                    string message = Encoding.UTF8.GetString(p.GetValueRaw(4)).Trim();
                    ShowTemporaryStatus(message);
                }
            }
        }

        private void UpdateDefaultStatus()
        {
            if (IsDead) { GameStatus = "ВЫ УМЕРЛИ"; return; }
            if (IsWinner) { GameStatus = "ПОБЕДА!"; return; }

            if (_currentTurnHolderName == _networkService.PlayerName)
                GameStatus = "Ваш ход";
            else if (!string.IsNullOrEmpty(_currentTurnHolderName))
                GameStatus = $"Ход игрока: {_currentTurnHolderName}";
            else
                GameStatus = "Ожидание...";
        }

        private async void ShowTemporaryStatus(string message)
        {
            _statusTimerTokenSource?.Cancel();
            _statusTimerTokenSource = new CancellationTokenSource();
            var token = _statusTimerTokenSource.Token;

            GameStatus = message;

            try
            {
                await Task.Delay(3000, token);
                UpdateDefaultStatus();
            }
            catch (TaskCanceledException) { }
        }

        private void CheckWinCondition()
        {
            if (IsDead) return;
            bool anyOpponentsJoined = Opponents.Any(o => o.IsConnected);
            int aliveOpponents = Opponents.Count(o => o.IsConnected && !o.IsDead);

            if (anyOpponentsJoined && aliveOpponents == 0)
            {
                IsWinner = true;
                IsMyTurn = false;
                MyPower = 0;
                GameStatus = "ПОБЕДА!";
            }
        }

        [RelayCommand]
        private void PlayCard(ICardMini card)
        {
            if (card == null || !IsMyTurn || HasPlayedCardThisTurn || IsDead) return;

            var packet = EAPacket.Create(5, 3);
            packet.SetValueRaw(3, BitConverter.GetBytes(card.CardId));
            _networkService.SendPacket(packet);

            MyHandCards.Remove(card);
            HasPlayedCardThisTurn = true;
        }

        [RelayCommand]
        private void EndTurn()
        {
            if (IsDead || IsWinner || !IsMyTurn) return;
            IsMyTurn = false;
            HasPlayedCardThisTurn = false;
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

            ShowTemporaryStatus($"Атака на {targetName} (-{dmg} HP)!");
        }

        [RelayCommand]
        private void BuyCard(ICardMini card)
        {
            if (card == null || !IsMyTurn || IsDead) return;

            var packet = EAPacket.Create(5, 4);
            packet.SetValueRaw(3, BitConverter.GetBytes(card.CardId));
            _networkService.SendPacket(packet);

            // Здесь мы специально ничего не удаляем и не добавляем в коллекции.
            // Ждем пакет GameStateUpdate (Тип 4, Подтип 1), который придет от сервера
            // и вызовет MyHandCards.Clear() + заполнит актуальным списком.
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