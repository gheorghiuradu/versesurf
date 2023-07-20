using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class TransformExtensions
    {
        public static IEnumerable<Transform> GetChildren(this GameObject parent) => GetChildren(parent.transform);

        public static IEnumerable<Transform> GetChildren(this Transform transform)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                yield return transform.GetChild(i);
            }
        }

        public static bool IsLastChild(this Transform transform) => transform.GetSiblingIndex() == (transform.parent.childCount - 1);
    }
}