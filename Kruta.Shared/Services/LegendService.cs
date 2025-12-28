using Kruta.Shared.Models.Cards;
using Kruta.Shared.Models.Cards.Familiar;
using Kruta.Shared.Models.Tokens.JKSToken;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Kruta.Shared.Services
{
    public class LegendService
    {
        private Random _random = new Random();
        private List<LegendCard> Legends = new List<LegendCard>();

        public LegendService()
        {
            LegendsInit();
        }

        private void LegendsInit()
        {
            // 1. Получаем путь к папке, где запущено приложение (bin/Debug/...)
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 2. Формируем чистый путь к файлу без упоминания Kruta.Shared
            string pathToJson = Path.Combine(baseDir, "JsonCards", "LegendCards.json");

            // 3. Проверка на наличие файла, чтобы избежать DirectoryNotFoundException
            if (!File.Exists(pathToJson))
            {
                // Выводим в дебаг, чтобы сразу видеть реальный путь в случае ошибки
                System.Diagnostics.Debug.WriteLine($"[ERROR] Файл легенд не найден: {pathToJson}");
                Legends = new List<LegendCard>();
                return;
            }

            try
            {
                string jsonString = File.ReadAllText(pathToJson);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true,
                    // Вот эта настройка разрешает комментарии в стиле C# (//)
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    // На всякий случай разрешим "висячие запятые" (запятая после последнего элемента)
                    AllowTrailingCommas = true
                };

                // Десериализация
                var legendTimely = JsonSerializer.Deserialize<List<LegendCard>>(jsonString, options);

                // Заполняем список (если null — создаем пустой)
                Legends = legendTimely ?? new List<LegendCard>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Ошибка десериализации легенд: {ex.Message}");
                Legends = new List<LegendCard>();
            }
        }

        //получить все карты легенд (допустим чтобы сформировать стопку в начале и потом с ней работать)
        public List<LegendCard> GetAllLegends() => new List<LegendCard>(Legends);

        //взять из стопки одну случайную легенду
        public LegendCard GetRandomLegend()
        {
            // Генерируем число от 0 до (Count - 1)
            int randomIndex = _random.Next(Legends.Count);

            // 3. Получение токена
            var legend = Legends[randomIndex];

            // Удаление конкретного Фамильяра из списка доступных
            // Это гарантирует, что каждый Фамильяр будет выдан только один раз
            Legends.RemoveAt(randomIndex);

            return legend;
        }


    }
}
