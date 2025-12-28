using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kruta.GUI2.Services;
using Kruta.Protocol;
using System.Text;
using System.Diagnostics;

namespace Kruta.GUI2.ViewModels
{
    public partial class RegistrationViewModel : ObservableObject, IDisposable
    {
        private readonly NetworkService _networkService;

        [ObservableProperty] private string _nickname;
        [ObservableProperty] private bool _isBusy;

        public RegistrationViewModel(NetworkService networkService)
        {
            _networkService = networkService;

            // Подписываемся на события сетевого сервиса
            _networkService.OnPacketReceived += HandleServerResponse;

            Debug.WriteLine("[RegVM] Инициализация: подписка на пакеты активна");
        }

        [RelayCommand]
        private async Task Register()
        {
            if (IsBusy || string.IsNullOrWhiteSpace(Nickname)) return;
            IsBusy = true;

            try
            {
                if (!await _networkService.ConnectAsync("127.0.0.1", 13000))
                {
                    await Shell.Current.DisplayAlert("Упс", "Сервер спит", "ОК");
                    return;
                }

                // КЛЮЧЕВАЯ СТРОКА: Запоминаем наш ник в Singleton сервисе
                _networkService.PlayerName = Nickname.Trim();

                var packet = EAPacket.Create(1, 0);
                packet.SetValueRaw(3, Encoding.UTF8.GetBytes(Nickname));

                _networkService.SendPacket(packet);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[RegVM] Ошибка при регистрации: {ex.Message}");
                IsBusy = false;
            }
        }

        private void HandleServerResponse(EAPacket packet)
        {
            Debug.WriteLine($"[RegVM] Обработка пакета: Type={packet.PacketType}, Subtype={packet.PacketSubtype}");

            if (packet.PacketType == 1 && packet.PacketSubtype == 0)
            {
                // Отписываемся сразу, чтобы не ловить пакеты во время перехода
                _networkService.OnPacketReceived -= HandleServerResponse;

                // Принудительно в главном потоке
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    try
                    {
                        Debug.WriteLine("[RegVM] Пытаюсь скрыть IsBusy и перейти на GamePage...");
                        IsBusy = false;

                        // Если маршрут "///GamePage" не зарегистрирован в AppShell.xaml, тут будет вылет
                        await Shell.Current.GoToAsync("GamePage");

                        Debug.WriteLine("[RegVM] Навигация успешно вызвана");
                    }
                    catch (Exception ex)
                    {
                        // ТУТ МЫ УВИДИМ РЕАЛЬНУЮ ПРИЧИНУ
                        Debug.WriteLine($"[RegVM] КРИТИЧЕСКАЯ ОШИБКА НАВИГАЦИИ: {ex.Message}");
                        Debug.WriteLine($"[RegVM] StackTrace: {ex.StackTrace}");

                        // Если упало, вернем IsBusy, чтобы кнопка ожила
                        IsBusy = false;
                    }
                });
            }
        }

        // Метод для очистки ресурсов
        private void Cleanup()
        {
            _networkService.OnPacketReceived -= HandleServerResponse;
            Debug.WriteLine("[RegVM] Отписка от событий сети");
        }

        // Реализация IDisposable на случай, если VM будет уничтожена системой
        public void Dispose()
        {
            Cleanup();
        }
    }
}