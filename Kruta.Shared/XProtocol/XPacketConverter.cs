using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Kruta.Shared.XProtocol
{
    public static class XPacketConverter
    {
        // === МЕТОД ДЛЯ ПОЛУЧЕНИЯ ПОЛЕЙ, ПОМЕЧЕННЫХ АТРИБУТОМ ===
        private static List<Tuple<FieldInfo, byte>> GetFields(Type t)
        {
            return t.GetFields(BindingFlags.Instance |
                               BindingFlags.NonPublic |
                               BindingFlags.Public)
            .Where(field => field.GetCustomAttribute<XFieldAttribute>() != null)
            .Select(field => Tuple.Create(field, field.GetCustomAttribute<XFieldAttribute>().FieldID))
            .ToList();
        }

        // === СЕРИАЛИЗАЦИЯ (Класс -> XPacket) ===

        public static XPacket Serialize(XPacketType type, object obj, bool strict = false)
        {
            var tuple = XPacketTypeManager.GetType(type);
            var packet = XPacket.Create(tuple.Item1, tuple.Item2);

            var fields = GetFields(obj.GetType());

            if (strict)
            {
                // Проверка на повторное использование ID поля
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

            foreach (var field in fields)
            {
                // Устанавливаем значение, используя методы XPacket
                packet.SetValue(field.Item2, field.Item1.GetValue(obj));
            }

            return packet;
        }

        // === ДЕСЕРИАЛИЗАЦИЯ (XPacket -> Класс) ===

        public static T Deserialize<T>(XPacket packet, bool strict = false) where T : new()
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

                // Вызов GetValue<T> через Reflection для передачи нужного типа
                var value = typeof(XPacket)
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
