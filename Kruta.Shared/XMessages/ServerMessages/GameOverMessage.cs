using Kruta.Shared.XProtocol;
using System.Text;

namespace Kruta.Shared.XMessages.ServerMessages
{
    public class GameOverMessage
    {
        // --- Поля для XPacketConverter (Value Types) ---
        [XField(1)]
        public int WinnerPlayerId; // ID победителя

        [XField(2)]
        public int TotalTurnsPlayed; // Общее количество ходов

        // --- Поле, которое нужно обрабатывать вручную (String) ---
        public string FinalScoreSummary;

        private const byte SummaryFieldId = 3;

        // === РУЧНАЯ СЕРИАЛИЗАЦИЯ/ДЕСЕРИАЛИЗАЦИЯ ===

        public XPacket SerializeString(XPacket packet)
        {
            packet.SetValueString(SummaryFieldId, FinalScoreSummary);
            return packet;
        }

        public void DeserializeString(XPacket packet)
        {
            FinalScoreSummary = packet.GetValueString(SummaryFieldId);
        }
    }
}