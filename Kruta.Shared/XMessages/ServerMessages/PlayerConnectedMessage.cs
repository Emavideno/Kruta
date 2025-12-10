using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.XProtocol;

namespace Kruta.Shared.XMessages.ServerMessages
{
    public class PlayerConnectedMessage
    {
        // Поле 1: ID подключенного игрока (ВАШ ID 1-4)
        [XField(1)]
        public int PlayerId;

        // Поле 2: Индекс слота игрока (полезно для клиента)
        [XField(2)]
        public int SlotIndex;

        // Поле 3 (РУЧНАЯ СЕРИАЛИЗАЦИЯ): Имя игрока
        public string PlayerName;
        private const byte NameFieldId = 3;

        // === РУЧНАЯ СЕРИАЛИЗАЦИЯ/ДЕСЕРИАЛИЗАЦИЯ ===

        public XPacket SerializeString(XPacket packet)
        {
            packet.SetValueString(NameFieldId, PlayerName);
            return packet;
        }

        public void DeserializeString(XPacket packet)
        {
            PlayerName = packet.GetValueString(NameFieldId);
        }
    }
}
