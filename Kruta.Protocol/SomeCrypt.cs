using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Protocol
{
    public static class SomeCrypt
    {
        //шифрование на основе алгоритма Виженера, но для байтовых данных
        //Это логическое развитие XOR-шифрования
        public static byte[] Encrypt(byte[] data, string passPhrase)
        {
            if (string.IsNullOrEmpty(passPhrase)) return data;

            byte[] result = new byte[data.Length]; //создаем новый массив той же длины
            byte[] keyBytes = Encoding.UTF8.GetBytes(passPhrase); //получаем байты кодовой фразы

            for (int i = 0; i < data.Length; i++)
            {
                // Применяем XOR (исключающее или) байта данных с байтом ключа
                // Ключ циклически повторяется через оператор %
                result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
            }

            return result;
        }

        public static byte[] Decrypt(byte[] data, string passPhrase)
        {
            // Для XOR-шифрования дешифровка выполняется тем же самым алгоритмом
            return Encrypt(data, passPhrase);
        }
    }
}
