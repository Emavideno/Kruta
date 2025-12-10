using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Kruta.Shared.XProtocol
{
    /// <summary>
    /// Обработчик шифрования/расшифровки данных, использующий современный алгоритм AES (замена RijndaelManaged).
    /// Использует PBKDF2 для вывода ключа и криптографически стойкие генераторы для Salt и IV.
    /// </summary>
    public static class RijndaelHandler
    {
        private const int Keysize = 256;              // Размер ключа в битах (для AES-256)
        private const int KeyByteSize = Keysize / 8;  // 32 байта для ключа и соли
        private const int IvByteSize = 16;            // 16 байт для IV (размер блока AES)
        private const int DerivationIterations = 1000;

        /// <summary>
        /// Шифрует массив байтов, добавляя Salt и IV к результату.
        /// Итоговый формат пакета: [SALT (32)] + [IV (16)] + [CIPHERTEXT]
        /// </summary>
        public static byte[] Encrypt(byte[] data, string passPhrase)
        {
            var saltStringBytes = GenerateCryptographicallyRandomBytes(KeyByteSize); // 32 байта
            var ivStringBytes = GenerateCryptographicallyRandomBytes(IvByteSize);   // 16 байт

            // 1. Вывод ключа (Key Derivation) с использованием PBKDF2
            var keyBytes = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(passPhrase),
                saltStringBytes,
                DerivationIterations,
                HashAlgorithmName.SHA256,
                KeyByteSize);

            using (var symmetricKey = Aes.Create())
            {
                symmetricKey.KeySize = Keysize;
                symmetricKey.BlockSize = IvByteSize * 8; // 128 бит
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;

                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(data, 0, data.Length);
                            cryptoStream.FlushFinalBlock();

                            // 2. Формирование итогового пакета
                            var cipherTextBytes = saltStringBytes;
                            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();

                            return cipherTextBytes;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Расшифровывает массив байтов, извлекая Salt и IV из начала пакета.
        /// </summary>
        public static byte[] Decrypt(byte[] data, string passPhrase)
        {
            // 1. Извлечение SALT (32 байта) и IV (16 байт)
            var saltStringBytes = data.Take(KeyByteSize).ToArray();
            var ivStringBytes = data.Skip(KeyByteSize).Take(IvByteSize).ToArray();
            var cipherTextBytes = data.Skip(KeyByteSize + IvByteSize).ToArray();

            // 2. Восстановление ключа с использованием PBKDF2
            var keyBytes = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(passPhrase),
                saltStringBytes,
                DerivationIterations,
                HashAlgorithmName.SHA256,
                KeyByteSize);

            using (var symmetricKey = Aes.Create())
            {
                symmetricKey.KeySize = Keysize;
                symmetricKey.BlockSize = IvByteSize * 8; // 128 бит
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;

                using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream(cipherTextBytes))
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var plainTextBytes = new byte[cipherTextBytes.Length];
                            var read = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);

                            // 3. Возвращаем только прочитанные (расшифрованные) байты
                            return plainTextBytes.Take(read).ToArray();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Генерирует криптографически случайный массив байтов.
        /// </summary>
        private static byte[] GenerateCryptographicallyRandomBytes(int size)
        {
            var randomBytes = new byte[size];
            RandomNumberGenerator.Fill(randomBytes);
            return randomBytes;
        }
    }
}