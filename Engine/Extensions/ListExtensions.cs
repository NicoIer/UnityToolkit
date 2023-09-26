using System.Collections.Generic;

namespace UnityToolkit
{
    public static class ListExtensions
    {
        public static TElement Random<TElement>(this TElement[] array)
        {
            if (array == null || array.Length <= 0)
            {
                return default;
            }

            return array[UnityEngine.Random.Range(0, array.Length)];
        }
        
        public static TElement Random<TElement>(this IList<TElement> list)
        {
            if (list == null || list.Count <= 0)
            {
                return default;
            }

            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        
        
    }
}