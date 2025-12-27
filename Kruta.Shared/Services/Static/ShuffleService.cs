using Kruta.Shared.Models.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kruta.Shared.Services.Static
{
    public static class ShuffleService
    {
        private static readonly Random _random = new Random();

        public static IEnumerable<T> ShuffleCustom<T>(this IEnumerable<T> source) where T : ICard
        {
            // 1. Создаем массив из исходной коллекции для реализации обмена
            T[] array = source.ToArray();
            int n = array.Length;

            // 2. Реализация алгоритма Фишера-Йетса(gpt)
            while (n > 1)
            {
                n--;
                // Выбор случайного индекса k от 0 до n
                int k = _random.Next(n + 1);

                // Обмен (swap)
                T value = array[k];
                array[k] = array[n];
                array[n] = value;
            }

            // Возвращаем перемешанный массив
            return array;
        }
    }
}
