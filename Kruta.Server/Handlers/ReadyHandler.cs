using Kruta.Protocol;
using Kruta.Server.Handlers.Interface;
using System.Text;

namespace Kruta.Server.Handlers
{
    public class ReadyHandler : IPacketHandler
    {
        public void Handle(ClientObject client, EAPacket packet)
        {
            // 1. Переключаем состояние готовности текущего игрока
            client.IsReady = !client.IsReady;
            Console.WriteLine($"[LOBBY] Игрок {client.Username} (ID: {client.Id}) теперь готов: {client.IsReady}");

            // 2. Формируем пакет уведомления (Type 3, Subtype 1)
            // Поле 3: Ник игрока, Поле 4: Байт статуса (1 или 0)
            var statusPacket = EAPacket.Create(3, 1);
            statusPacket.SetValueRaw(3, Encoding.UTF8.GetBytes(client.Username));
            statusPacket.SetValueRaw(4, new byte[] { (byte)(client.IsReady ? 1 : 0) });

            // Получаем доступ к серверу через объект клиента
            var server = client._gameServer2;

            lock (server.Clients)
            {
                // 3. Рассылаем статус этого игрока ВСЕМ в лобби
                foreach (var c in server.Clients)
                {
                    c.Send(statusPacket);
                }

                // 4. Проверяем условие начала игры
                // Минимум 2 игрока и все имеют IsReady == true
                if (server.Clients.Count >= 2 && server.Clients.All(c => c.IsReady))
                {
                    StartGame(server);
                }
            }
        }

        private void StartGame(GameServer2 server)
        {
            Console.WriteLine("[GAME] Условие выполнено: Все игроки готовы. Запуск игры!");

            // Пакет команды старта (Type 4, Subtype 0)
            var startPacket = EAPacket.Create(4, 0);

            foreach (var c in server.Clients)
            {
                c.Send(startPacket);
            }
        }
    }
}