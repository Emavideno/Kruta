using System;
using System.Collections.Generic;
using System.Text;
using Kruta.Shared.XProtocol;

namespace Kruta.Shared.XMessages.ServerMessages
{
    public class ErrorMessage
    {
        // --- Поля для XPacketConverter (Value Types) ---
        [XField(1)]
        public int ErrorCode; // Например, 401, 500, 404

        // --- Поле, которое нужно обрабатывать вручную (String) ---
        public string Message;

        private const byte ErrorMessageFieldId = 2;

        // === РУЧНАЯ СЕРИАЛИЗАЦИЯ/ДЕСЕРИАЛИЗАЦИЯ ===

        public XPacket SerializeString(XPacket packet)
        {
            packet.SetValueString(ErrorMessageFieldId, Message);
            return packet;
        }

        public void DeserializeString(XPacket packet)
        {
            Message = packet.GetValueString(ErrorMessageFieldId);
        }
    }
}
