using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Kruta.Shared.XProtocol
{
    public static class XPacketTypeManager
    {
        private static readonly Dictionary<XPacketType, Tuple<byte, byte>> TypeDictionary =
            new Dictionary<XPacketType, Tuple<byte, byte>>();

        static XPacketTypeManager()
        {
            // === РЕГИСТРАЦИЯ ВСЕХ ТИПОВ ===

            // 0: Unknown (должен быть 0, 0)
            RegisterType(XPacketType.Unknown, 0, 0);

            // 1: СЕРВИСНЫЕ (Handshake)
            RegisterType(XPacketType.Handshake, 1, 0);

            // 2: КЛИЕНТ (Auth, PlayCard, BuyCard, EndTurn)
            RegisterType(XPacketType.Auth, 2, 0);
            RegisterType(XPacketType.PlayCard, 2, 1);
            RegisterType(XPacketType.BuyCard, 2, 2);
            RegisterType(XPacketType.EndTurn, 2, 3);

            // 3: СЕРВЕР (GameStateUpdate, GameOver, Error)
            RegisterType(XPacketType.GameStateUpdate, 3, 0);
            RegisterType(XPacketType.GameOver, 3, 1);
            RegisterType(XPacketType.Error, 3, 2);
            RegisterType(XPacketType.PlayerConnected, 3, 3);
        }

        public static void RegisterType(XPacketType type, byte btype, byte bsubtype)
        {
            if (TypeDictionary.ContainsKey(type))
            {
                throw new Exception($"Packet type {type:G} is already registered.");
            }

            TypeDictionary.Add(type, Tuple.Create(btype, bsubtype));
        }

        public static Tuple<byte, byte> GetType(XPacketType type)
        {
            if (!TypeDictionary.ContainsKey(type))
            {
                throw new Exception($"Packet type {type:G} is not registered.");
            }

            return TypeDictionary[type];
        }

        public static XPacketType GetTypeFromPacket(XPacket packet)
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

            return XPacketType.Unknown;
        }
    }
}
