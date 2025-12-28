using Kruta.Shared.Network.Messages.ClientMessages;
using Kruta.Shared.Network.Enums;
using Kruta.TestClient;

Console.Title = "Kruta Test Client";
var client = new ClientService();
await client.ConnectAsync();

if (client != null)
{
    // 1. Отправляем сообщение для регистрации (AuthMessage)
    var authMsg = new AuthMessage { PlayerName = "Kolobok-Hacker" };
    await client.SendMessageAsync(authMsg);

    // 2. Имитация хода (Отправляем PlayCard)
    // Важно: тут используется заглушка CardId=100, в реальной игре нужно ID из GameState
    await Task.Delay(1000);
    var playCardMsg = new PlayCardMessage { CardId = 100, TargetPlayerId = null };
    await client.SendMessageAsync(playCardMsg);

    // 3. Имитация покупки карты
    await Task.Delay(1000);
    var buyCardMsg = new BuyCardMessage { CardId = 201, Source = BuySource.Baraholka };
    await client.SendMessageAsync(buyCardMsg);

    // 4. Имитация завершения хода
    await Task.Delay(1000);
    await client.SendMessageAsync(new EndTurnMessage());

    // Держим консоль открытой для приема сообщений от сервера
    Console.WriteLine("Нажмите Enter для выхода...");
    Console.ReadLine();
}