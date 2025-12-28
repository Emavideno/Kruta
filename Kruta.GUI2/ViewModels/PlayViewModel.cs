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

            //// МЕНЯТЬ ТУТ
            IsMyTurn = true;
            GameStatus = "ТЕСТОВЫЙ РЕЖИМ: КНОПКА ДОЛЖНА БЫТЬ";
        }

        // Вынесите логику в обертку
        private void HandlePacketWrapper(EAPacket p)
        {
            MainThread.BeginInvokeOnMainThread(() => HandlePacket(p));
        }

        private void HandlePacket(EAPacket p)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Получен пакет: Type={p.PacketType}, Subtype={p.PacketSubtype}");

            ////// Тип 5: Управление ходом
            //if (p.PacketType == 5)
            //{
            //    IsMyTurn = true;
            //    GameStatus = "Ваш ход!";
            //}

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

                // ПРОВЕРКА: Если я первый в списке игроков, я хожу первым
                // _allPlayerNames[0] - это всегда тот, кто подключился первым
                if (_allPlayerNames.Count > 0 && _allPlayerNames[0] == _networkService.PlayerName)
                {
                    IsMyTurn = true;
                    GameStatus = "Вы ходите первым!";
                }
                else
                {
                    IsMyTurn = false;
                    GameStatus = "Ожидание хода первого игрока...";
                }
            }

            if (p.PacketType == 5 && p.PacketSubtype == 1)
            {
                string activePlayerName = "";
                if (p.HasField(3))
                {
                    activePlayerName = Encoding.UTF8.GetString(p.GetValueRaw(3)).Trim();
                }

                MainThread.BeginInvokeOnMainThread(() => {
                    // Если имя в пакете совпадает с моим — это мой ход!
                    IsMyTurn = (activePlayerName == _networkService.PlayerName);

                    GameStatus = IsMyTurn ? "ВАШ ХОД!" : $"Ходит {activePlayerName}...";
                });
            }
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

        [RelayCommand]
        private void EndTurn()
        {
            IsMyTurn = false;
            GameStatus = "Ожидание хода противника...";

            // Отправляем пакет "Конец хода" (Type 5, Subtype 0)
            var packet = EAPacket.Create(5, 0);
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