using System;

namespace Nico
{
    public class MinHeap<TElement>
    {
        private MaxHeap<TElement> _maxHeap;
        internal TElement[] elements => _maxHeap.elements;
        public int Count => _maxHeap.Count;
        public int Capacity => _maxHeap.Capacity;

        public MinHeap(Func<TElement, TElement, int> comparer)
        {
            int Comparer(TElement a, TElement b) => -comparer(a, b);
            _maxHeap = new MaxHeap<TElement>(Comparer);
        }

        public void Insert(TElement element) => _maxHeap.Insert(element);


        public TElement Pop() => _maxHeap.Pop();

        public TElement Peek() => _maxHeap.Peek();


        public void Clear() => _maxHeap.Clear();
    }
}