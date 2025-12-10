using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace Kruta.Shared.XProtocol
{
    public static class XProtocolEncryptor
    {
        private static string Key { get; } = "2e985f930853919313c96d001cb5701f";

        public static byte[] Encrypt(byte[] data)
        {
            // Использование нового RijndaelHandler
            return RijndaelHandler.Encrypt(data, Key);
        }

        public static byte[] Decrypt(byte[] data)
        {
            // Использование нового RijndaelHandler
            return RijndaelHandler.Decrypt(data, Key);
        }
    }
}