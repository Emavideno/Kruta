using Kruta.Shared.Models.Cards.Bespredel;
using Kruta.Shared.Models.Cards.Creature;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Kruta.Shared.Services
{
    public class BespredelService
    {
        private List<BespredelCard> Bespredels = new(); //стопка карт беспределов
        private List<BespredelCard> BespredelsDiscard = new(); //стопка сбросов беспределов

        Random _random = new Random();

        public BespredelService()
        {
            InitBespredel();
        }

        private void InitBespredel()
        {
            // 1. Получаем базовый путь к папке приложения
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 2. Формируем универсальный путь к JsonCards
            string pathToJson = Path.Combine(baseDir, "JsonCards", "BespredelCards.json");

            // 3. Проверка на наличие файла
            if (!File.Exists(pathToJson))
            {
                System.Diagnostics.Debug.WriteLine($"[КРИТИЧЕСКАЯ ОШИБКА] Файл карт беспредела не найден: {pathToJson}");
                Bespredels = new List<BespredelCard>();
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
                var bespredelesTimely = JsonSerializer.Deserialize<List<BespredelCard>>(jsonString, options);

                // Присваиваем результат или пустой список
                Bespredels = bespredelesTimely ?? new List<BespredelCard>();
            }
            catch (Exception ex)
            {
                // Выводим ошибку десериализации в окно Output
                System.Diagnostics.Debug.WriteLine($"[ОШИБКА JSON] Ошибка при десериализации Беспредела: {ex.Message}");
                Bespredels = new List<BespredelCard>();
            }
        }

        //получить 1 случайное тварь
        public BespredelCard GetRandomBespredelCard()
        {
            // Генерируем число от 0 до (Count - 1)
            int randomIndex = _random.Next(Bespredels.Count);

            // 3. Получение токена
            BespredelCard bespredelesRandom = Bespredels[randomIndex];

            // Удаление конкретного Фамильяра из списка доступных
            // Это гарантирует, что каждый Фамильяр будет выдан только один раз
            Bespredels.RemoveAt(randomIndex);
            BespredelsDiscard.Add(bespredelesRandom); //и добавление в стопку сбросов беспределов

            return bespredelesRandom;
        }

        //метод отдающий все беспределы (для добавления в общую колоду)
        public List<BespredelCard> GetAllBespredel()
        {
            return new List<BespredelCard>(Bespredels);
        }
    }
}
