using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Kruta.Protocol
{
    public static class EAPacketTypeManager
    {
        private static readonly Dictionary<EAPacketType, Tuple<byte, byte>> TypeDictionary
                                = new Dictionary<EAPacketType, Tuple<byte, byte>>();

        public static void RegisterType(EAPacketType type, byte btype, byte bsubtype)
        {
            if (TypeDictionary.ContainsKey(type)) //если по такому ключу уже зареганыы
            {
                throw new Exception();
            }

            TypeDictionary.Add(type, Tuple.Create(btype, bsubtype));
        }

        //Зная тип получаем его в байтах
        public static Tuple<byte, byte> GetType(EAPacketType type)
        {
            if (!TypeDictionary.ContainsKey(type))
            {
                throw new Exception();
            }

            return TypeDictionary[type];
        }

        //по пакету узнаем его тип
        public static EAPacketType GetTypeFromPacket(EAPacket packet)
        {
            var type = packet.PacketType;
            var subtype = packet.PacketSubtype;

            foreach (var tuple in TypeDictionary)
            {
                var value = tuple.Value;

                if (value.Item1 == type && value.Item2 == subtype)
                {
                    return tuple.Key;
                }
            }

            return EAPacketType.Unknown;
        }

    }
}
