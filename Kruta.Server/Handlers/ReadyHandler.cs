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
                // Запускаем игру только если она еще не начата
                if (!server.IsGameStarted && server.Clients.Count >= 2 && server.Clients.All(c => c.IsReady))
                {
                    server.IsGameStarted = true; // Фиксируем старт
                    StartGame(server);
                }
            }
        }

        private void StartGame(GameServer2 server)
        {
            Console.WriteLine("[GAME] Генерация общей колоды и инициализация...");

            var rnd = new Random();

            // 1. Наполняем серверную колоду (server.MainDeck)
            server.MainDeck.Clear();

            // Вялые палочки (1), Пшики (3), Палочки (6), Знаки (7) — по 10 штук
            int[] commonIds = { 1, 3, 6, 7 };
            foreach (int id in commonIds)
            {
                for (int i = 0; i < 10; i++) server.MainDeck.Add(id);
            }

            // Остальные (2, 4, 5, 8, 9, 10) — по 3 штуки
            int[] rareIds = { 2, 4, 5, 8, 9, 10 };
            foreach (int id in rareIds)
            {
                for (int i = 0; i < 3; i++) server.MainDeck.Add(id);
            }

            // 2. Перемешиваем колоду (Fisher-Yates shuffle)
            int n = server.MainDeck.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                int value = server.MainDeck[k];
                server.MainDeck[k] = server.MainDeck[n];
                server.MainDeck[n] = value;
            }

            // 3. Берем 5 карт для барахолки из начала перемешанной колоды
            server.Baraholka = server.MainDeck.Take(5).ToArray();
            server.MainDeck.RemoveRange(0, 5);

            // 5. Рассылаем игрокам данные
            lock (server.Clients)
            {
                for (int i = 0; i < server.Clients.Count; i++)
                {
                    var targetClient = server.Clients[i];
                    var initPacket = EAPacket.Create(4, 1);

                    initPacket.SetValueRaw(5, BitConverter.GetBytes(i));

                    // Барахолка из сервера
                    byte[] baraholkaBytes = new byte[20];
                    Buffer.BlockCopy(server.Baraholka, 0, baraholkaBytes, 0, 20);
                    initPacket.SetValueRaw(6, baraholkaBytes);

                    // Раздача карт из ОБЩЕЙ колоды
                    int[] playerStartCards = server.MainDeck.Take(12).ToArray();
                    server.MainDeck.RemoveRange(0, 12);

                    // Актуальный остаток
                    initPacket.SetValueRaw(8, BitConverter.GetBytes(server.MainDeck.Count));

                    byte[] startCardsBytes = new byte[playerStartCards.Length * 4];
                    Buffer.BlockCopy(playerStartCards, 0, startCardsBytes, 0, startCardsBytes.Length);
                    initPacket.SetValueRaw(7, startCardsBytes);

                    targetClient.Send(initPacket);
                    Console.WriteLine($"[GAME] Данные отправлены {targetClient.Username}. Выдано 12 карт.");
                }

                int finalCount = server.MainDeck.Count;
                var finalDeckPacket = EAPacket.Create(4, 1); // Или создайте подтип для обновления колоды
                finalDeckPacket.SetValueRaw(8, BitConverter.GetBytes(finalCount));

                foreach (var c in server.Clients)
                {
                    c.Send(finalDeckPacket);
                }
            }


            // 5. Сигнал к началу игры (смена экрана)
            var startSignal = EAPacket.Create(4, 0);
            lock (server.Clients)
            {
                foreach (var c in server.Clients) c.Send(startSignal);
            }

            Console.WriteLine("[GAME] Игра запущена успешно.");
        }
    }
}