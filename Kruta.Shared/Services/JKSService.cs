using Kruta.Shared.Models.Tokens.JKSToken;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Kruta.Shared.Services
{
    public class JKSService
    {
        private Random _random = new Random();
        private List<WizardPropertyToken> JKS = new List<WizardPropertyToken>();

        public JKSService()
        {
            JKSInit();
        }

        private void JKSInit()
        {
            // Получаем путь к папке, где запущено приложение
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // Собираем путь: папка_приложения/JsonCards/WizardPropertyTokens.json
            string pathToJson = Path.Combine(baseDir, "JsonCards", "WizardPropertyTokens.json");

            if (!File.Exists(pathToJson))
            {
                // Для отладки: выведем в консоль, где именно мы искали файл
                throw new FileNotFoundException($"Файл не найден по пути: {pathToJson}");
            }

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

            try
            {
                JKS = JsonSerializer.Deserialize<List<WizardPropertyToken>>(jsonString, options) ?? new();
            }
            catch (Exception ex)
            {
                // Важно: выводим саму ошибку ex, чтобы понять что не так в JSON
                Console.WriteLine($"Ошибка десериализации ЖКС: {ex.Message}");
            }
        }

        public WizardPropertyToken GetRandomJksForPlayer()
        {
            // Генерируем число от 0 до (Count - 1)
            int randomIndex = _random.Next(JKS.Count);

            // 3. Получение токена
            var token = JKS[randomIndex];

            // Удаление конкретного ЖКС из списка доступных
            // Это гарантирует, что каждый жетон будет выдан только один раз
            JKS.RemoveAt(randomIndex);

            return token;
        }
    }
}
