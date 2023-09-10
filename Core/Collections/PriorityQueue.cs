using System;
using System.Collections.Generic;

namespace Nico
{
    // 优先级队列 优先级越高 则 越先出队
    public class PriorityQueue<TElement>
    {
        internal readonly MaxHeap<TElement> maxHeap;
        internal readonly Func<TElement, TElement, int> comparer;
        public int Count => maxHeap.Count;

        public PriorityQueue(Func<TElement, TElement, int> comparer)
        {
            this.comparer = comparer;
            maxHeap = new MaxHeap<TElement>(comparer);
        }
        
        public IEnumerable<TElement> Enumerate()
        {
            for (int i = 0; i < maxHeap.Count; i++)
            {
                yield return maxHeap.elements[i];
            }
        }

        public TElement Peek()
        {
            return maxHeap.Peek();
        }
        public bool TryDeQueue(out TElement element)
        {
            if (Count == 0)
            {
                element = default;
                return false;
            }

            element = Dequeue();
            return true;
        }
        
        public bool TryPeek(out TElement element)
        {
            if (Count == 0)
            {
                element = default;
                return false;
            }

            element = Peek();
            return true;
        }

        public void Enqueue(TElement element)
        {
            maxHeap.Insert(element);
        }

        public TElement Dequeue()
        {
            return maxHeap.Pop();
        }
    }
}