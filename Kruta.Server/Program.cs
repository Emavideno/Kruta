using Kruta.Server.Networking;

Console.Title = "Kruta Game Server";

// 1. Создаем экземпляр сервера
var server = new GameServer();

// 2. Запускаем сервер асинхронно
var serverTask = server.StartAsync();

// 3. Запускаем цикл ожидания команды на остановку
Console.WriteLine("\nНажмите Enter или 'Q' для остановки сервера...");
Console.WriteLine("------------------------------------------------");

// Ожидание ввода команды
var input = Console.ReadLine();

// Проверяем, была ли введена 'Q' или просто Enter
if (input != null && input.Equals("Q", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Получена команда 'Q'. Завершение работы...");
}

// 4. Вызываем метод Stop()
server.Stop();

// 5. Ожидаем завершения задачи сервера (ServerTask),
// чтобы убедиться, что цикл AcceptTcpClientAsync завершен корректно.
try
{
    await serverTask;
}
catch (Exception ex)
{
    // Игнорируем ожидаемые исключения, вызванные server.Stop()
    // (например, SocketException из-за _listener.Stop())
    Console.WriteLine($"[INFO] Задача сервера завершилась. {ex.Message}");
}

Console.WriteLine("Сервер успешно завершил работу.");