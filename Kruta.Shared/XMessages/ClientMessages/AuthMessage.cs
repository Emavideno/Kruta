using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.XProtocol;

namespace Kruta.Shared.XMessages.ClientMessages
{
    public class AuthMessage
    {
        // --- Поля для XPacketConverter (Value Types) ---
        [XField(1)]
        public int ProtocolVersion = 1;

        // --- Поле, которое нужно обрабатывать вручную (String) ---
        public string PlayerName;

        // Поле 3 (ID): Зарезервируем для имени игрока при ручной сериализации
        private const byte PlayerNameFieldId = 3;

        // === РУЧНАЯ СЕРИАЛИЗАЦИЯ/ДЕСЕРИАЛИЗАЦИЯ ===

        // Метод, который нужно вызывать после XPacketConverter.Serialize
        public XPacket SerializeString(XPacket packet)
        {
            packet.SetValueString(PlayerNameFieldId, PlayerName);
            return packet;
        }

        // Метод, который нужно вызывать после XPacketConverter.Deserialize
        public void DeserializeString(XPacket packet)
        {
            PlayerName = packet.GetValueString(PlayerNameFieldId);
        }
    }
}
