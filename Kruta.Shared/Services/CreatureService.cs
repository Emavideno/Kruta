using Kruta.Shared.Models.Cards.Creature;
using Kruta.Shared.Models.Cards.Spell;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Kruta.Shared.Services
{
    public class CreatureService
    {
        private List<CreatureCard> Creatures = new(); //стопка карт тварей

        Random _random = new Random();

        public CreatureService()
        {
            InitCreature();
        }

        private void InitCreature()
        {
            // 1. Получаем путь к папке, где запущено приложение
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 2. Формируем путь (благодаря <Link> в csproj, ищем напрямую в JsonCards)
            string pathToJson = Path.Combine(baseDir, "JsonCards", "CreatureCards.json");

            // 3. Проверка на наличие файла
            if (!File.Exists(pathToJson))
            {
                System.Diagnostics.Debug.WriteLine($"[КРИТИЧЕСКАЯ ОШИБКА] Файл карт тварей не найден: {pathToJson}");
                Creatures = new List<CreatureCard>();
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
                var creaturesTimely = JsonSerializer.Deserialize<List<CreatureCard>>(jsonString, options);

                // Присваиваем результат или пустой список
                Creatures = creaturesTimely ?? new List<CreatureCard>();
            }
            catch (Exception ex)
            {
                // Выводим реальную причину ошибки в окно вывода Visual Studio
                System.Diagnostics.Debug.WriteLine($"[ОШИБКА JSON] Ошибка при десериализации Тварей: {ex.Message}");
                Creatures = new List<CreatureCard>();
            }
        }

        //получить 1 случайное тварь
        public CreatureCard GetRandomCreatureCard()
        {
            // Генерируем число от 0 до (Count - 1)
            int randomIndex = _random.Next(Creatures.Count);

            // 3. Получение токена
            CreatureCard creatureRandom = Creatures[randomIndex];

            // Удаление конкретного Фамильяра из списка доступных
            // Это гарантирует, что каждый Фамильяр будет выдан только один раз
            Creatures.RemoveAt(randomIndex);

            return creatureRandom;
        }

        //метод отдающий все заклинания (для добавления в общую колоду)
        public List<CreatureCard> GetAllCreatures()
        {
            return new List<CreatureCard>(Creatures);
        }

    }
}
