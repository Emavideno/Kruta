using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kruta.GUI2.Services;
using Kruta.Protocol;
using System.Collections.ObjectModel;
using System.Text;

namespace Kruta.GUI2.ViewModels
{
    public partial class GameViewModel : ObservableObject
    {
        private readonly NetworkService _networkService;

        // Список игроков для отображения в CollectionView
        public ObservableCollection<PlayerDisplay> Players { get; } = new();

        public GameViewModel(NetworkService networkService)
        {
            _networkService = networkService;

            for (int i = 0; i < 4; i++)
            {
                Players.Add(new PlayerDisplay { Name = "Свободно" });
            }

            // Подписываемся на получение пакетов
            _networkService.OnPacketReceived += (p) =>
            {
                // 1. ПОЛУЧЕНИЕ СПИСКА / ДОБАВЛЕНИЕ НОВОГО ИГРОКА (Type 2, Subtype 1)
                if (p.PacketType == 2 && p.PacketSubtype == 1) // PlayerListUpdate
                {
                    var rawName = p.GetValueRaw(3);
                    if (rawName == null) return;
                    string name = Encoding.UTF8.GetString(rawName);

                    MainThread.BeginInvokeOnMainThread(() => {
                        // Логика: ищем первый свободный слот ИЛИ проверяем, нет ли уже такого игрока
                        var existingPlayer = Players.FirstOrDefault(x => x.Name == name);
                        if (existingPlayer == null)
                        {
                            // Ищем первый "пустой" слот (где имя "Свободно" или пустое)
                            var emptySlot = Players.FirstOrDefault(x => x.Name == "Свободно" || string.IsNullOrEmpty(x.Name));
                            if (emptySlot != null)
                            {
                                emptySlot.Name = name;
                            }
                            else
                            {
                                // Если свободных слотов нет, просто добавляем в список
                                Players.Add(new PlayerDisplay { Name = name });
                            }
                        }
                    });
                }

                // 2. ОБНОВЛЕНИЕ СТАТУСА ГОТОВНОСТИ (Type 3, Subtype 1)
                else if (p.PacketType == 3 && p.PacketSubtype == 1)
                {
                    var rawName = p.GetValueRaw(3);
                    var rawStatus = p.GetValueRaw(4);

                    if (rawName == null || rawStatus == null) return;

                    string name = Encoding.UTF8.GetString(rawName);
                    bool ready = rawStatus[0] == 1;

                    MainThread.BeginInvokeOnMainThread(() => {
                        var player = Players.FirstOrDefault(x => x.Name == name);
                        if (player != null)
                        {
                            player.IsReady = ready;
                        }
                    });
                }

                // 3. КОМАНДА НАЧАЛА ИГРЫ (Type 4, Subtype 0)
                else if (p.PacketType == 4 && p.PacketSubtype == 0)
                {
                    MainThread.BeginInvokeOnMainThread(async () => {
                        // Переходим на страницу игрового поля
                        await Shell.Current.GoToAsync("PlayPage");
                    });
                }
            };

            // Как только страница загрузилась, запрашиваем у сервера актуальный список игроков
            _networkService.SendPacket(EAPacket.Create(2, 0));
        }

        // Команда для кнопки "ГОТОВ"
        [RelayCommand]
        private void ToggleReady()
        {
            // Отправляем запрос на сервер изменить наш статус
            _networkService.SendPacket(EAPacket.Create(3, 0));
        }
    }

    // Класс-помощник для отображения игрока в списке
    public partial class PlayerDisplay : ObservableObject
    {
        [ObservableProperty]
        private string _name = "Свободно";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StatusText))]
        [NotifyPropertyChangedFor(nameof(StatusColor))]
        private bool _isReady;

        public string StatusText => IsReady ? "ГОТОВ" : "Ожидание...";
        public Color StatusColor => IsReady ? Colors.Green : Colors.Gray;
    }
}