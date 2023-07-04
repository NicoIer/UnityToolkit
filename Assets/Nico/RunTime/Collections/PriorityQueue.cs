using System;
using System.Runtime.CompilerServices;

namespace Nico
{
    // 优先级队列 优先级越高 则 越先出队
    public class PriorityQueue<TElement>
    {
        private MaxHeap<TElement> _maxHeap;
        public int Count => _maxHeap.Count;

        public PriorityQueue(Func<TElement, TElement, int> comparer)
        {
            _maxHeap = new MaxHeap<TElement>(comparer);
        }

        public TElement Peek()
        {
            return _maxHeap.Peek();
        }
        
        public bool TryDeQueue(out TElement element)
        {
            if (Count == 0)
            {
                element = default;
                return false;
            }

            element = DeQueue();
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
        
        public void EnQueue(TElement element)
        {
            _maxHeap.Insert(element);
        }

        public TElement DeQueue()
        {
            return _maxHeap.Pop();
        }
    }
}