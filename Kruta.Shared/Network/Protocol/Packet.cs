using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.Network.Enums;
using System.Text.Json;

namespace Kruta.Shared.Network.Protocol
{
    public class Packet
    {
        public MessageType Type { get; set; }
        public string Payload { get; set; } // JSON строка конкретного сообщения

        // Пустой конструктор для десериализации
        public Packet() { }

        // Метод для упаковки любого сообщения в Пакет
        public static Packet Create<T>(T message) where T : IMessage
        {
            return new Packet
            {
                Type = message.Type,
                Payload = JsonSerializer.Serialize(message)
            };
        }

        // Метод для распаковки (достаем конкретный класс из Payload)
        public T Deserialize<T>()
        {
            return JsonSerializer.Deserialize<T>(Payload);
        }
    }
}
