using System.Net.Sockets;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Text;
using Kruta.Shared.XProtocol;
using Kruta.Shared.XMessages;
using Kruta.Shared.XMessages.ClientMessages;
using Kruta.Shared.XMessages.ServerMessages;
using Kruta.TestClient;

Console.Title = "Kruta Test Client";

// --- Константы для подключения ---
// Убедитесь, что эти параметры соответствуют вашему серверу
const string ServerIp = "127.0.0.1";
const int ServerPort = 13000;
const string PlayerName = "Kolobok-Hacker";

Console.WriteLine($"Kruta Test Client: {PlayerName}");
Console.WriteLine($"Подключение к {ServerIp}:{ServerPort}...");

// Создаем экземпляр ClientService с именем игрока
var client = new ClientService(PlayerName);

// Запускаем подключение и основной цикл.
// Этот метод не завершится, пока клиент не будет отключен или не введена команда выхода (X).
await client.ConnectAndRun(ServerIp, ServerPort);

Console.WriteLine("Клиентская программа завершена. Нажмите Enter, чтобы закрыть консоль...");
Console.ReadLine();