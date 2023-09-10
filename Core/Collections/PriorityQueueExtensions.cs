using System.Collections.Generic;

namespace Nico
{
    public static class PriorityQueueExtensions
    {
        public static IEnumerable<TElement> EnumerateMinToMax<TElement>(this PriorityQueue<TElement> queue)
        {
            if (HeapStore<TElement>.minHeap == null)
            {
                HeapStore<TElement>.minHeap = new MinHeap<TElement>(queue.comparer);
            }

            HeapStore<TElement>.minHeap.Clear();

            foreach (var element in queue.Enumerate())
            {
                HeapStore<TElement>.minHeap.Insert(element);
            }

            while (HeapStore<TElement>.minHeap.Count > 0)
            {
                yield return HeapStore<TElement>.minHeap.Pop();
            }
        }

        public static IEnumerable<TElement> EnumerateMaxToMin<TElement>(this PriorityQueue<TElement> queue)
        {
            if (HeapStore<TElement>.maxHeap == null)
            {
                HeapStore<TElement>.maxHeap = new MaxHeap<TElement>(queue.comparer);
            }

            HeapStore<TElement>.maxHeap.Clear();

            foreach (var element in queue.Enumerate())
            {
                HeapStore<TElement>.maxHeap.Insert(element);
            }

            while (HeapStore<TElement>.maxHeap.Count > 0)
            {
                yield return HeapStore<TElement>.maxHeap.Pop();
            }
        }

        private static class HeapStore<TElement>
        {
            public static MinHeap<TElement> minHeap;
            public static MaxHeap<TElement> maxHeap;
        }
    }
}