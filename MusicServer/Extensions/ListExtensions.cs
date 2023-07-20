using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicServer.Extensions
{
    public static class ListExtensions
    {
        public static void Shuffle<T>(this IList<T> list, Random random)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = random.Next(0, n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static IList<T> Clone<T>(this IList<T> listToClone) where T : System.ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }
}