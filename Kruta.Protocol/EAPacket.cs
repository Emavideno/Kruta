using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Kruta.Protocol
{
    public class EAPacket
    {
        public byte PacketType { get; private set; } //тип пакета
        public byte PacketSubtype { get; private set; } //подтип пакета
        public List<EAPacketField> Fields { get; set; } = new(); //набор полей

        private bool ChangeHeaders { get; set; } //заголовок пакета: зашифрованный true (CS а не EA) или обычный false

        public bool Protected { get; set; } //защищенный или нет
        //защищенный пакет будет использовать в заголовке аббривиатуру CS (то есть C Sharp)
        //а это в байтах выглядит как 0x43 и 0x53

        private EAPacket() { } //приватный конструктор

        public static EAPacket Create(byte type, byte subtype)
        {
            return new EAPacket()
            {
                PacketType = type,
                PacketSubtype = subtype
            };
        }

        public byte[] ToPacket()
        {
            var packet = new MemoryStream();

            //0x45 == E, 0x41==A
            packet.Write(ChangeHeaders ?
                     new byte[] { 0x43, 0x53, PacketType, PacketSubtype} : //MS -при шифровании
                     new byte[] { 0x45, 0x41, PacketType, PacketSubtype }, 0, 4); //EA

            // Сортируем поля по ID
            var fields = Fields.OrderBy(field => field.FieldID);

            // Записываем поля
            foreach (var field in fields)
            {
                packet.Write(new[] { field.FieldID, field.FieldSize }, 0, 2);
                packet.Write(field.Contents, 0, field.Contents.Length);
            }

            // Записываем конец пакета
            packet.Write(new byte[] { 0x41, 0x45 }, 0, 2); //AE

            return packet.ToArray();
        }

        public static EAPacket Parse(byte[] packet , bool encrypted = false)
        {
            if (packet.Length < 6) return null;

            // Находим первое вхождение маркера конца пакета (AE) — позволяет парсить буфер, который
            // может содержать несколько пакетов или хвостовые байты.
            int endIndex = -1;
            for (int i = 4; i < packet.Length - 1; i++)
            {
                if (packet[i] == 0x41 && packet[i + 1] == 0x45)
                {
                    endIndex = i + 1; // индекс последнего байта маркера AE
                    break;
                }
            }

            if (endIndex == -1) return null; // полного пакета ещё нет в буфере

            // Берём только байты до конца первого пакета (включительно)
            var packetSlice = packet.Take(endIndex + 1).ToArray();

            // Проверяем заголовок
            if (packetSlice[0] == 0x43 && packetSlice[1] == 0x53) encrypted = true;
            else if (packetSlice[0] != 0x45 || packetSlice[1] != 0x41) return null;

            var eapacket = Create(packetSlice[2], packetSlice[3]);
            var fieldsData = packetSlice.Skip(4).Take(packetSlice.Length - 6).ToArray();

            int pointer = 0;
            while (pointer < fieldsData.Length)
            {
                if (fieldsData.Length - pointer < 2) break;
                byte id = fieldsData[pointer];
                byte size = fieldsData[pointer + 1];

                byte[] contents = size > 0 ? fieldsData.Skip(pointer + 2).Take(size).ToArray() : null;
                eapacket.Fields.Add(new EAPacketField { FieldID = id, FieldSize = size, Contents = contents });

                pointer += 2 + size;
            }

            // Если пакет был зашифрован (CS), извлекаем внутренний пакет из поля 0
            return encrypted ? DecryptPacket(eapacket) : eapacket;
        }

        //Объект преобразуем в массив байтов (value не может иметь string, List и тд). Только int, double и тд
        public byte[] FixedObjectToByteArray(object value) 
        {
            var rawsize = Marshal.SizeOf(value);
            var rawdata = new byte[rawsize];

            var handle = GCHandle.Alloc(rawdata,
                GCHandleType.Pinned);

            Marshal.StructureToPtr(value,
                handle.AddrOfPinnedObject(),
                false);

            handle.Free();

            return rawdata;
        }

        //поток байт преобразуем в объект (value type именно, потому что они имеют фиксированный размер в памяти)
        private T ByteArrayToFixedObject<T>(byte[] bytes) where T : struct
        {
            T structure;

            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try
            {
                structure = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }

            return structure;
        }

        //поиск поля по его id 
        public EAPacketField GetField(byte id)
        {
            foreach (var field in Fields)
            {
                if (field.FieldID == id)
                {
                    return field;
                }
            }

            return null;
        }

        //проверка существования поля по его id
        public bool HasField(byte id)
        {
            return GetField(id) != null;
        }

        //Получаем значение поля (его контент, уже в виде объекта C#) по его id
        public T GetValue<T>(byte id) where T : struct
        {
            var field = GetField(id);

            if (field == null)
            {
                throw new Exception();
            }
            var neededSize = Marshal.SizeOf(typeof(T));

            if (field.FieldSize != neededSize)
            {
                throw new Exception();
            }

            return ByteArrayToFixedObject<T>(field.Contents);
        }

        //Записываем готовый объект C# в значение поля
        public void SetValue(byte id, object structure)
        {
            if (!structure.GetType().IsValueType)
            {
                throw new Exception("Only value types are available.");
            }

            var field = GetField(id);

            if (field == null) //если по такому id не существует поля то создаем значит
            {
                field = new EAPacketField
                {
                    FieldID = id
                };

                Fields.Add(field);
            }

            var bytes = FixedObjectToByteArray(structure);

            if (bytes.Length > byte.MaxValue)
            {
                throw new Exception("Object is too big. Max length is 255 bytes.");
            }

            field.FieldSize = (byte)bytes.Length;
            field.Contents = bytes;
        }

        //Добавляем возможность в поле (по его id) записать массив данных
        //Массив - ссылочный тип, а до этого мы принимали только value type
        //Поэтому как бы создаем отдельный метод
        public void SetValueRaw(byte id, byte[] rawData)
        {
            var field = GetField(id);

            if (field == null)
            {
                field = new EAPacketField
                {
                    FieldID = id
                };

                Fields.Add(field);
            }

            if (rawData.Length > byte.MaxValue)
            {
                throw new Exception("Object is too big. Max length is 255 bytes.");
            }

            field.FieldSize = (byte)rawData.Length;
            field.Contents = rawData;
        }

        //Получение данных из поля по его id, если мы знаем что там записан
        //Массив данных, который мы хотим получить
        public byte[] GetValueRaw(byte id)
        {
            var field = GetField(id);

            if (field == null)
            {
                throw new Exception();
            }

            return field.Contents;
        }

        //удобный метод вызова шифрования
        public EAPacket Encrypt()
        {
            return EncryptPacket(this);
        }

        //удобный метод вызова дешифрования
        public EAPacket Decrypt()
        {
            return DecryptPacket(this);
        }


        private static EAPacket DecryptPacket(EAPacket packet)
        {
            if (!packet.HasField(0))
            {
                return null;
            }

            //Получаем данные (1), расшифровываем(2) и парсим заново (3)

            var rawData = packet.GetValueRaw(0); //Мы храним наш пакет в зашифрованном виде как field с id=0 в другом пакете.  достаёт зашифрованные байты
            var decrypted = EAProtocolEncryptor.Decrypt(rawData); //вызываем метод дешифровки. превращает их обратно в нормальные байты-данные пакета.


            return Parse(decrypted, false); //байты пакета превращаем в объект уже
        }

        //шифруем пакетик
        public static EAPacket EncryptPacket(EAPacket packet)
        {
            if (packet == null)
            {
                return null;
            }

            var rawBytes = packet.ToPacket(); //превращаем в байты
            var encrypted = EAProtocolEncryptor.Encrypt(rawBytes); //шифруем байты

            var p = Create(0, 0); //создаем другой пакет с id 0 
            p.SetValueRaw(0, encrypted); //засовываем массив байтов в field с id=0
            p.ChangeHeaders = true; //говорим что этот пакетик имеет шифрование

            return p;
        }
    }
}
