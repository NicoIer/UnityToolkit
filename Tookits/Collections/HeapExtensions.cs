using System.Collections.Generic;

namespace Nico.Collections
{
    public static class HeapExtensions
    {
        //迭代,但是不保证顺序
        public static IEnumerable<TElement> Enumerate<TElement>(this MaxHeap<TElement> maxHeap)
        {
            for (int i = 0; i < maxHeap.Count; i++)
            {
                yield return maxHeap.elements[i];
            }
        }
        
        public static IEnumerable<TElement> Enumerate<TElement>(this MinHeap<TElement> minHeap)
        {
            for (int i = 0; i < minHeap.Count; i++)
            {
                yield return minHeap.elements[i];
            }
        }
    }
}