using System;
using System.Collections.Generic;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Kruta.Shared.XProtocol
{
    public class XPacket
    {
        // === ЗАГОЛОВКИ И КОНЦОВИКИ ===
        private static readonly byte[] Header = { 0xAF, 0xAA, 0xAF };
        private static readonly byte[] EncryptedHeader = { 0x95, 0xAA, 0xFF };
        private static readonly byte[] Ending = { 0xFF, 0x00 };

        // === СВОЙСТВА ПАКЕТА ===
        public byte PacketType { get; private set; }
        public byte PacketSubtype { get; private set; }
        public List<XPacketField> Fields { get; set; } = new List<XPacketField>();
        public bool ChangeHeaders { get; private set; } // Для шифрования

        // === КОНСТРУКТОРЫ И ФАБРИКА ===
        private XPacket() { }

        public static XPacket Create(byte type, byte subtype)
        {
            return new XPacket
            {
                PacketType = type,
                PacketSubtype = subtype
            };
        }

        // === ПРЕОБРАЗОВАНИЕ ПАКЕТА (Сериализация в байты) ===

        public byte[] ToPacket()
        {
            using (var packet = new MemoryStream())
            {
                // 1. Заголовок, Тип и Подтип
                var headerToWrite = ChangeHeaders ? EncryptedHeader : Header;
                packet.Write(headerToWrite, 0, headerToWrite.Length);
                packet.Write(new[] { PacketType, PacketSubtype }, 0, 2);

                // 2. Поля (сортировка по FieldID)
                var fields = Fields.OrderBy(field => field.FieldID);

                foreach (var field in fields)
                {
                    packet.Write(new[] { field.FieldID, field.FieldSize }, 0, 2);
                    if (field.Contents != null && field.Contents.Length > 0)
                    {
                        packet.Write(field.Contents, 0, field.Contents.Length);
                    }
                }

                // 3. Конец пакета
                packet.Write(Ending, 0, 2);

                return packet.ToArray();
            }
        }

        // === ПАРСИНГ ПАКЕТА (Десериализация из байтов) ===

        public static XPacket Parse(byte[] packet, bool markAsEncrypted = false)
        {
            if (packet.Length < 7) return null; // Минимальный размер: 3 + 1 + 1 + 2

            var encrypted = false;
            var header = new byte[3];
            Array.Copy(packet, 0, header, 0, 3);

            // 1. Проверка заголовка
            if (header.SequenceEqual(Header))
            {
                encrypted = false;
            }
            else if (header.SequenceEqual(EncryptedHeader))
            {
                encrypted = true;
            }
            else
            {
                return null; // Неизвестный заголовок
            }

            // 2. Проверка концовки
            var endIndex = packet.Length - 2;
            if (packet[endIndex] != Ending[0] || packet[endIndex + 1] != Ending[1])
            {
                return null; // Неверная концовка
            }

            var type = packet[3];
            var subtype = packet[4];
            var xpacket = Create(type, subtype);
            xpacket.ChangeHeaders = encrypted; // Сохраняем информацию о шифровании

            // Если пакет зашифрован, его нужно сначала расшифровать
            if (encrypted && !markAsEncrypted)
            {
                // Делаем рекурсивный вызов DecryptPacket
                return DecryptPacket(xpacket);
            }

            // 3. Парсинг полей
            var fieldsBytes = packet.Skip(5).ToArray();
            var offset = 0;

            while (offset < fieldsBytes.Length - 2) // -2 на Ending
            {
                var id = fieldsBytes[offset];
                var size = fieldsBytes[offset + 1];

                var contents = size != 0
                    ? fieldsBytes.Skip(offset + 2).Take(size).ToArray()
                    : null;

                xpacket.Fields.Add(new XPacketField
                {
                    FieldID = id,
                    FieldSize = size,
                    Contents = contents
                });

                offset += 2 + size; // Сдвиг на ID (1) + SIZE (1) + Contents (size)
            }

            return xpacket;
        }

        // === ДОСТУП К ПОЛЯМ ===

        public XPacketField GetField(byte id)
        {
            return Fields.FirstOrDefault(field => field.FieldID == id);
        }

        public bool HasField(byte id)
        {
            return GetField(id) != null;
        }

        // === УТИЛИТЫ ДЛЯ ПРЕОБРАЗОВАНИЯ ТИПОВ ===

        private byte[] FixedObjectToByteArray(object value)
        {
            // Здесь реализация из статьи с использованием Marshal
            var rawsize = Marshal.SizeOf(value);
            var rawdata = new byte[rawsize];

            var handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            handle.Free();

            return rawdata;
        }

        private T ByteArrayToFixedObject<T>(byte[] bytes) where T : struct
        {
            // Здесь реализация из статьи с использованием Marshal
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

        // === ГЕТТЕРЫ ДЛЯ Value-Type ===

        public T GetValue<T>(byte id) where T : struct
        {
            var field = GetField(id);

            if (field == null)
            {
                throw new Exception($"Field with ID {id} wasn't found.");
            }
            var neededSize = Marshal.SizeOf(typeof(T));

            if (field.FieldSize != neededSize)
            {
                throw new Exception($"Can't convert field to type {typeof(T).FullName}.\n" +
                                    $"We have {field.FieldSize} bytes but we need exactly {neededSize}.");
            }

            return ByteArrayToFixedObject<T>(field.Contents);
        }

        // === СЕТТЕРЫ ДЛЯ Value-Type ===

        public void SetValue(byte id, object structure)
        {
            if (!structure.GetType().IsValueType)
            {
                throw new Exception("Only value types are available.");
            }

            var field = GetField(id) ?? new XPacketField { FieldID = id };

            var bytes = FixedObjectToByteArray(structure);

            if (bytes.Length > byte.MaxValue)
            {
                throw new Exception("Object is too big. Max length is 255 bytes.");
            }

            field.FieldSize = (byte)bytes.Length;
            field.Contents = bytes;

            if (GetField(id) == null)
            {
                Fields.Add(field);
            }
        }

        // === RAW (Байты) ГЕТТЕРЫ/СЕТТЕРЫ (для шифрования) ===

        public void SetValueRaw(byte id, byte[] rawData)
        {
            var field = GetField(id) ?? new XPacketField { FieldID = id };

            if (rawData.Length > byte.MaxValue)
            {
                throw new Exception("Raw data is too big. Max length is 255 bytes.");
            }

            field.FieldSize = (byte)rawData.Length;
            field.Contents = rawData;

            if (GetField(id) == null)
            {
                Fields.Add(field);
            }
        }

        public byte[] GetValueRaw(byte id)
        {
            var field = GetField(id);

            if (field == null)
            {
                throw new Exception($"Field with ID {id} wasn't found.");
            }

            return field.Contents;
        }

        // === МЕТОДЫ ШИФРОВАНИЯ/РАСШИФРОВКИ ===

        public XPacket Encrypt()
        {
            return EncryptPacket(this);
        }

        public XPacket Decrypt()
        {
            return DecryptPacket(this);
        }

        public static XPacket EncryptPacket(XPacket packet)
        {
            if (packet == null) return null;

            var rawBytes = packet.ToPacket();
            var encrypted = XProtocolEncryptor.Encrypt(rawBytes);

            var p = Create(0, 0);
            p.SetValueRaw(0, encrypted); // Зашифрованные данные всегда в поле ID 0
            p.ChangeHeaders = true;

            return p;
        }

        private static XPacket DecryptPacket(XPacket packet)
        {
            if (!packet.HasField(0))
            {
                return null;
            }

            var rawData = packet.GetValueRaw(0);
            var decrypted = XProtocolEncryptor.Decrypt(rawData);

            // Рекурсивный парсинг расшифрованных данных, помеченных как уже расшифрованные
            return Parse(decrypted, true);
        }

        // === СЕТТЕР ДЛЯ СТРОКИ ===
        public void SetValueString(byte id, string value)
        {
            if (value == null)
            {
                // Можно передать пустой массив для null-строки или выбросить исключение
                SetValueRaw(id, Array.Empty<byte>());
                return;
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            SetValueRaw(id, bytes); // Используем SetValueRaw, который принимает byte[]
        }

        // === ГЕТТЕР ДЛЯ СТРОКИ ===
        public string GetValueString(byte id)
        {
            var bytes = GetValueRaw(id);

            if (bytes == null || bytes.Length == 0)
            {
                return string.Empty; // Возвращаем пустую строку, если данных нет
            }

            // Декодируем байты в строку с помощью UTF8
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
