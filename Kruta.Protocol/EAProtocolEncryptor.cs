using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Protocol
{
    public class EAProtocolEncryptor
    {
        private static string Key { get; } = "TimerkhanAglyamovich"; //2e985f930853919313c96d001cb5701f

        public static byte[] Encrypt(byte[] data)
        {
            return SomeCrypt.Encrypt(data, Key); 
        }

        public static byte[] Decrypt(byte[] data)
        {
            return SomeCrypt.Decrypt(data, Key); 
        }
    }
}
