using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Kruta.Protocol.Serilizations
{
    public class EAPacketConverter
    {
        //Получаем абсолютно все поля типа которые мы передаем, помеченные атрибутом EAField
        private static List<Tuple<FieldInfo, byte>> GetFields(Type t)
        {
            return t.GetFields(BindingFlags.Instance |
                               BindingFlags.NonPublic |
                               BindingFlags.Public)
            .Where(field => field.GetCustomAttribute<EAFieldAttribute>() != null)
            .Select(field => Tuple.Create(field, field.GetCustomAttribute<EAFieldAttribute>().FieldID))
            .ToList();
        }

        public static EAPacket Serialize(byte type, byte subtype, object obj, bool strict = false)
        {
            var fields = GetFields(obj.GetType()); //получаем все поля типа помеченные атрибутом

            if (strict) //жесткий режим == если у полей совпадают id, то получаем исключение
            {
                var usedUp = new List<byte>();

                foreach (var field in fields)
                {
                    if (usedUp.Contains(field.Item2))
                    {
                        throw new Exception("One field used two times.");
                    }

                    usedUp.Add(field.Item2);
                }
            }

            var packet = EAPacket.Create(type, subtype); //создаем пакет

            foreach (var field in fields)
            {
                packet.SetValue(field.Item2, field.Item1.GetValue(obj));
            }

            return packet;
        }

        //Перегруженный метод сериализации, чтобы вместо двух байтов типа и подтипа можно было
        //просто передать тип из Enum
        public static EAPacket Serialize(EAPacketType type, object obj, bool strict = false)
        {
            var ids = EAPacketTypeManager.GetType(type); //получаем 2 байта для типа и подтипа

            //вызываем базовый метод с байтами
            return Serialize(ids.Item1, ids.Item2, obj, strict);
        }

        //десериализация (из всего пакета мы хотим достать один определенный тип T)
        public static T Deserialize<T>(EAPacket packet, bool strict = false)
        {
            var fields = GetFields(typeof(T));
            var instance = Activator.CreateInstance<T>();

            if (fields.Count == 0)
            {
                return instance;
            }

            foreach (var tuple in fields)
            {
                var field = tuple.Item1;
                var packetFieldId = tuple.Item2;

                if (!packet.HasField(packetFieldId))
                {
                    if (strict)
                    {
                        throw new Exception($"Couldn't get field[{packetFieldId}] for {field.Name}");
                    }

                    continue;
                }

                var value = typeof(EAPacket)
                    .GetMethod("GetValue")?
                    .MakeGenericMethod(field.FieldType)
                    .Invoke(packet, new object[] { packetFieldId });

                if (value == null)
                {
                    if (strict)
                    {
                        throw new Exception($"Couldn't get value for field[{packetFieldId}] for {field.Name}");
                    }

                    continue;
                }

                field.SetValue(instance, value);
            }

            return instance;
        }
    }
}
