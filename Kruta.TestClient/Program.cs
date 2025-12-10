using System;
using System.Threading.Tasks;
using Kruta.TestClient;

// --- Константы для подключения ---
const string ServerIp = "127.0.0.1";
const int ServerPort = 13000;

Console.Title = "Kruta Test Client";
Console.WriteLine("===================================");
Console.WriteLine("=== Kruta Test Client v1.0      ===");
Console.WriteLine("===================================");

// 1. ЗАПРОС ИМЕНИ ИГРОКА У ПОЛЬЗОВАТЕЛЯ
Console.Write("Введите имя игрока (например, 'Player1'): ");
string playerNameInput = Console.ReadLine();

// Обработка пустого ввода
if (string.IsNullOrWhiteSpace(playerNameInput))
{
    // Используем случайное имя, если ничего не введено
    playerNameInput = $"Guest_{DateTime.Now.Millisecond}";
    Console.WriteLine($"Имя не введено. Используем имя по умолчанию: {playerNameInput}");
}

// Устанавливаем заголовок окна с фактическим именем
Console.Title = $"Kruta Test Client: {playerNameInput}";

Console.WriteLine($"Подключение к {ServerIp}:{ServerPort}...");

// 2. Создаем экземпляр ClientService с введенным именем
var client = new ClientService(playerNameInput);

// 3. Запускаем подключение и основной цикл.
await client.ConnectAndRun(ServerIp, ServerPort);

Console.WriteLine("Клиентская программа завершена. Нажмите Enter, чтобы закрыть консоль...");
Console.ReadLine();