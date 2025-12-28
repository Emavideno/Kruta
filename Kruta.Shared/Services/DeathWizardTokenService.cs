using Kruta.Shared.Models.Cards.Familiar;
using Kruta.Shared.Models.Tokens.DeathWizardToken;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Kruta.Shared.Services
{
    public class DeathWizardTokenService
    {
        private Random _random = new Random();
        private List<DeathWizardToken> DWT = new();

        public DeathWizardTokenService()
        {
            InitDWT();
        }

        private void InitDWT()
        {
            // 1. Точка отсчета — папка с запущенным приложением
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            // 2. Путь через универсальный комбинированный метод (без Kruta.Shared)
            string pathToJson = Path.Combine(baseDir, "JsonCards", "DeathWizardTokens.json");

            // 3. Безопасная проверка существования
            if (!File.Exists(pathToJson))
            {
                System.Diagnostics.Debug.WriteLine($"[КРИТИЧЕСКАЯ ОШИБКА] Файл жетонов дохлых колдунов не найден: {pathToJson}");
                DWT = new List<DeathWizardToken>();
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

                // Десериализация в список
                var dwtTimely = JsonSerializer.Deserialize<List<DeathWizardToken>>(jsonString, options);

                // Присвоение результата (или пустого списка, если в JSON пусто)
                DWT = dwtTimely ?? new List<DeathWizardToken>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ОШИБКА JSON] Не удалось распарсить жетоны дохлых колдунов: {ex.Message}");
                DWT = new List<DeathWizardToken>();
            }
        }

        public DeathWizardToken GetRandomDWTForPlayer()
        {
            // Генерируем число от 0 до (Count - 1)
            int randomIndex = _random.Next(DWT.Count);

            // 3. Получение токена
            var dwt = DWT[randomIndex];

            DWT.RemoveAt(randomIndex);

            return dwt;
        }
    }
}
