using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Server.Logic
{
    public class Player
    {
        public int Id { get; }
        public string Name { get; }

        // ID клиента, который управляет этим игроком (для сетевого взаимодействия)
        public int ClientId { get; }

        // TODO: Сюда можно добавить игровые поля (рука карт, ресурсы, статус)
        // public List<Card> Hand { get; set; }
        // public int ResourceCount { get; set; }

        public Player(int id, string name, int clientId)
        {
            Id = id;
            Name = name;
            ClientId = clientId;
        }

        public override string ToString()
        {
            return $"[Player {Id}] {Name} (Client ID: {ClientId})";
        }
    }
}
