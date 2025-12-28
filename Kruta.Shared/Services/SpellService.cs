using Kruta.Shared.Models.Cards.Familiar;
using Kruta.Shared.Models.Cards.Spell;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Kruta.Shared.Services
{
    public class SpellService
    {
        private List<SpellCard> Spells = new List<SpellCard>(); //стопка карт заклинаний
        
        Random _random = new Random();

        public SpellService()
        {
            InitSpells();
        }

        private void InitSpells()
        {
            // 1. Получаем путь к папке запуска приложения
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 2. Формируем универсальный путь (папка Kruta.Shared больше не нужна в пути)
            string pathToJson = Path.Combine(baseDir, "JsonCards", "SpellCards.json");

            // 3. Проверка на существование файла
            if (!File.Exists(pathToJson))
            {
                System.Diagnostics.Debug.WriteLine($"[КРИТИЧЕСКАЯ ОШИБКА] Файл заклинаний не найден: {pathToJson}");
                Spells = new List<SpellCard>();
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
                var spellsTimely = JsonSerializer.Deserialize<List<SpellCard>>(jsonString, options);

                // Присваиваем результат или пустой список
                Spells = spellsTimely ?? new List<SpellCard>();
            }
            catch (Exception ex)
            {
                // Выводим реальную причину ошибки десериализации
                System.Diagnostics.Debug.WriteLine($"[ОШИБКА JSON] Ошибка при десериализации Заклинаний: {ex.Message}");
                Spells = new List<SpellCard>();
            }
        }

        //получить 1 случайное заклинание
        public SpellCard GetRandomSpellCard()
        {
            // Генерируем число от 0 до (Count - 1)
            int randomIndex = _random.Next(Spells.Count);

            // 3. Получение токена
            SpellCard spellRandom = Spells[randomIndex];

            // Удаление конкретного Фамильяра из списка доступных
            // Это гарантирует, что каждый Фамильяр будет выдан только один раз
            Spells.RemoveAt(randomIndex);

            return spellRandom;
        }

        //метод отдающий все заклинания (для добавления в общую колоду)
        public List<SpellCard> GetAllSpells()
        {
            return new List<SpellCard>(Spells);
        }
        
    }
}
