using Kruta.Protocol;
using Kruta.Server.Handlers.Interface;
using System.Text;

namespace Kruta.Server.Handlers
{
    public class PlayerListHandler : IPacketHandler
    {
        public void Handle(ClientObject client, EAPacket packet)
        {
            Console.WriteLine($"[LIST] Клиент {client.Id} запросил список игроков.");

            // Доступ к списку всех клиентов через ссылку на сервер в ClientObject
            // (Убедись, что у твоего ClientObject есть публичное поле/свойство Server)
            var allClients = client._gameServer2.Clients;

            lock (allClients)
            {
                foreach (var otherClient in allClients)
                {
                    // Создаем пакет для каждого игрока
                    var updatePacket = EAPacket.Create(2, 1);

                    // Кладём ID (или ник) в поле 3, как ждет твой GameViewModel
                    string info = string.IsNullOrEmpty(otherClient.Username)
                                  ? otherClient.Id
                                  : otherClient.Username;

                    updatePacket.SetValueRaw(3, Encoding.UTF8.GetBytes(info));

                    // Отправляем запросившему клиенту
                    client.Send(updatePacket);
                }
            }
            Console.WriteLine($"[LIST] Список из {allClients.Count} чел. отправлен клиенту {client.Id}");
        }
    }
}