using Kruta.Shared.Models.Cards.Familiar;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Kruta.Shared.Services
{
    public class FamiliarService
    {
        private Random _random = new Random();
        private List<FamiliarCard> familiars = new List<FamiliarCard>();

        public FamiliarService()
        {
            FamiliarInit();
        }

        private void FamiliarInit() // инициализация всех Фамильяров
        {
            // 1. Получаем базовый путь к папке с запущенным приложением
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 2. Формируем путь к файлу (папка Kruta.Shared в пути не нужна благодаря <Link> в csproj)
            string pathToJson = Path.Combine(baseDir, "JsonCards", "FamiliarCards.json");

            // 3. Проверка на существование файла перед чтением
            if (!File.Exists(pathToJson))
            {
                // Выбрасываем исключение с указанием пути, чтобы в дебаггере сразу увидеть, где ошибка
                throw new FileNotFoundException($"Файл фамильяров не найден по пути: {pathToJson}");
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
                var familiarsTimely = JsonSerializer.Deserialize<List<FamiliarCard>>(jsonString, options);

                // Присваиваем результат, если десериализация прошла успешно (иначе пустой список)
                familiars = familiarsTimely ?? new List<FamiliarCard>();
            }
            catch (Exception ex)
            {
                // Используем Debug.WriteLine, чтобы видеть ошибку в консоли вывода Visual Studio
                System.Diagnostics.Debug.WriteLine($"Ошибка при десериализации Фамильяров: {ex.Message}");
                familiars = new List<FamiliarCard>();
            }
        }

        public FamiliarCard GetRandomFamiliarForPlayer()
        {
            // Генерируем число от 0 до (Count - 1)
            int randomIndex = _random.Next(familiars.Count);

            // 3. Получение токена
            var familiar = familiars[randomIndex];

            // Удаление конкретного Фамильяра из списка доступных
            // Это гарантирует, что каждый Фамильяр будет выдан только один раз
            familiars.RemoveAt(randomIndex);

            return familiar;
        }

    }
}
