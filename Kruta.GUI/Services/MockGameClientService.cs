using Kruta.GUI.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.GUI.Services
{
    public class MockGameClientService
    {
        // Имитируем успешное подключение и получение начального состояния
        public async Task<(bool success, GameState state)> ConnectAndGetStateAsync(string playerName)
        {
            // Заглушка: имитация задержки подключения
            await Task.Delay(1000);

            // Заглушка: если имя игрока "error", имитируем ошибку
            if (playerName.ToLower() == "error")
            {
                return (false, null);
            }

            var logicService = new MockGameLogicService();
            var initialState = logicService.CreateInitialMockState(playerName);

            return (true, initialState);
        }

        // Заглушка: отправка любого действия на сервер
        public Task SendActionAsync(object action)
        {
            // В реальной жизни тут будет сериализация в JSON и отправка по TCP
            System.Diagnostics.Debug.WriteLine($"[Client]: Action sent: {action.GetType().Name}");
            return Task.CompletedTask;
        }

        // Заглушка: отключение
        public void Disconnect()
        {
            System.Diagnostics.Debug.WriteLine($"[Client]: Disconnected.");
        }
    }
}
