using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Kruta.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Client.ViewModels
{
    public partial class RegistrationViewModel : ObservableObject
    {
        private readonly NetworkService _networkService;

        [ObservableProperty] private string _nickname;
        [ObservableProperty] private bool _isBusy;

        public RegistrationViewModel(NetworkService networkService)
        {
            _networkService = networkService;
            _networkService.PacketReceived += OnPacketReceived;
        }

        [RelayCommand]
        private async Task Register()
        {
            if (string.IsNullOrWhiteSpace(Nickname)) return;

            IsBusy = true;
            await _networkService.ConnectAsync("127.0.0.1", 8888);

            // Создаем пакет (Тип 1:0 - Регистрация)
            var packet = EAPacket.Create(1, 0);
            packet.SetValueRaw(3, Encoding.UTF8.GetBytes(Nickname));

            _networkService.SendPacket(packet);
        }

        private void OnPacketReceived(EAPacket packet)
        {
            // Проверяем, что это ответ на регистрацию (например, тип 1:1)
            if (packet.PacketType == 1 && packet.PacketSubtype == 1)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    IsBusy = false;
                    // Переходим на страницу игры
                    await Shell.Current.GoToAsync("///GamePage");
                });
            }
        }
    }
}
