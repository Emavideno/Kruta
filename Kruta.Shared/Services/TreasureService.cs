using Kruta.Shared.Models.Cards.Creature;
using Kruta.Shared.Models.Cards.Treasure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Kruta.Shared.Services
{
    public class TreasureService
    {
        private List<TreasureCard> Treasures = new(); //стопка карт сокровищ

        Random _random = new Random();

        public TreasureService()
        {
            InitTreasure();
        }

        private void InitTreasure()
        {
            // 1. Получаем путь к папке запуска приложения
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 2. Формируем универсальный путь (ищем напрямую в папке JsonCards в bin)
            string pathToJson = Path.Combine(baseDir, "JsonCards", "TreasureCards.json");

            // 3. Проверка на существование файла
            if (!File.Exists(pathToJson))
            {
                System.Diagnostics.Debug.WriteLine($"[КРИТИЧЕСКАЯ ОШИБКА] Файл сокровищ не найден: {pathToJson}");
                Treasures = new List<TreasureCard>();
                return;
            }

            try
            {
                string jsonString = File.ReadAllText(pathToJson);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                // Десериализация
                var treasuresTimely = JsonSerializer.Deserialize<List<TreasureCard>>(jsonString, options);

                // Присваиваем результат или пустой список
                Treasures = treasuresTimely ?? new List<TreasureCard>();
            }
            catch (Exception ex)
            {
                // Выводим конкретную причину ошибки в окно Output
                System.Diagnostics.Debug.WriteLine($"[ОШИБКА JSON] Ошибка при десериализации Сокровищ: {ex.Message}");
                Treasures = new List<TreasureCard>();
            }
        }

        //получить 1 случайное сокровище
        public TreasureCard GetRandomTreasureCard()
        {
            // Генерируем число от 0 до (Count - 1)
            int randomIndex = _random.Next(Treasures.Count);

            // 3. Получение токена
            TreasureCard treasureRandom = Treasures[randomIndex];

            // Удаление конкретного Фамильяра из списка доступных
            // Это гарантирует, что каждый Фамильяр будет выдан только один раз
            Treasures.RemoveAt(randomIndex);

            return treasureRandom;
        }

        //метод отдающий все заклинания (для добавления в общую колоду)
        public List<TreasureCard> GetAllTreasures()
        {
            return new List<TreasureCard>(Treasures);
        }
    }
}
