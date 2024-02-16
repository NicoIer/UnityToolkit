using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityToolkit
{
    public static class ListExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TElement Random<TElement>(this TElement[] array)
        {
            if (array == null || array.Length <= 0)
            {
                return default;
            }

            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TElement Random<TElement>(this IList<TElement> list)
        {
            if (list == null || list.Count <= 0)
            {
                return default;
            }

            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveTarget<T>(this List<T> list, Func<T, bool> condition)
        {
            int startPos = 0;
            int endPos = 0;
            while (startPos < list.Count && endPos < list.Count)
            {
                while (startPos < list.Count && !condition(list[startPos]))
                {
                    startPos++;
                }

                endPos = startPos;
                while (endPos < list.Count && condition(list[endPos]))
                {
                    endPos++;
                }

                if (startPos < list.Count && endPos < list.Count)
                {
                    list[startPos] = list[endPos];
                    list[endPos] = default;
                }
            }

            list.RemoveRange(startPos, list.Count - startPos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveNull<T>(this List<T> list) where T : class
        {
            list.RemoveTarget(e => e == null);
        }
    }
}