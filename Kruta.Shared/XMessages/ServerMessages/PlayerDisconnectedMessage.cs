using Kruta.Shared.XProtocol;
public class PlayerDisconnectedMessage
{
    // Сообщение, отправляемое сервером всем клиентам, когда игрок отключается.

    // Поле 1: ID отключенного игрока
    [XField(1)]
    public int PlayerId;

    public XPacket SerializeString(XPacket packet)
    {
        // Если вы строго следуете интерфейсу, этот метод должен быть, но он пуст
        return packet;
    }

    public void DeserializeString(XPacket packet) { }
}