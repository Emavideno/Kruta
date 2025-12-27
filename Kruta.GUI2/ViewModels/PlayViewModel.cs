using CommunityToolkit.Mvvm.ComponentModel;
using Kruta.GUI2.Services;
using Kruta.Protocol;
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

        public ObservableCollection<OpponentDisplay> Opponents { get; } = new()
        {
            new OpponentDisplay { Position = "Top" },
            new OpponentDisplay { Position = "Left" },
            new OpponentDisplay { Position = "Right" }
        };

        public PlayViewModel(NetworkService networkService)
        {
            _networkService = networkService;

            _networkService.OnPacketReceived += (p) =>
            {
                MainThread.BeginInvokeOnMainThread(() => HandlePacket(p));
            };

            // Запрашиваем актуальный список игроков при входе
            _networkService.SendPacket(EAPacket.Create(2, 0));
        }

        private void HandlePacket(EAPacket p)
        {
            // Тип 5: Управление ходом
            if (p.PacketType == 5)
            {
                GameStatus = "Ваш ход!";
            }

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
    }

    public partial class OpponentDisplay : ObservableObject
    {
        public string Position { get; set; }

        [ObservableProperty]
        private string _name = "Свободно";

        [ObservableProperty]
        private bool _isConnected = false;
    }
}