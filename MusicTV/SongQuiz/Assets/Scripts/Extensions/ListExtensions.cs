using Assets.Scripts.Reusable;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ListExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = Random.Range(0, n + 1);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static IList<T> Clone<T>(this IList<T> listToClone) where T : System.ICloneable
    {
        return listToClone.Select(item => (T)item.Clone()).ToList();
    }

    public static IEnumerable<T> Except<T>(this IEnumerable<T> enumerable, T item)
    {
        return enumerable.Except(new List<T> { item });
    }

    public static bool ContainsPlayer(this List<PlayerScript> list, string playerId)
    {
        return !(list.Find(p => string.Equals(p.Id, playerId)) is null);
    }
}