using System;
using System.Runtime.CompilerServices;

namespace Nico
{
    public class MaxHeap<TElement>
    {
        private const int DefaultCapacity = 4;
        private const int DefaultCapacityIncrease = 2;

        private readonly Func<TElement, TElement, int> _comparer;
        private TElement[] _elements;

        private int _count;
        public int Count => _count;
        public int Capacity => _elements.Length;

        public MaxHeap(Func<TElement, TElement, int> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentException($"Heap<{typeof(TElement)}>.comparer is null");
            }

            this._comparer = comparer;
            _elements = new TElement[DefaultCapacity];
            _count = 0;
        }

        public void Insert(TElement element)
        {
            if (_count == _elements.Length)
            {
                Grow();
            }

            _elements[_count] = element;
            HeapifyUp(_count);
            ++_count;
        }

        public TElement Peek()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Heap is empty");
            }

            return _elements[0];
        }

        public TElement Pop()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Heap is empty");
            }

            TElement result = _elements[0];
            --_count;
            _elements[0] = _elements[_count];
            HeapifyDown(0);
            return result;
        }

        public void Clear()
        {
            _count = 0;
        }

        private void HeapifyUp(int index)
        {
            int parentIndex = (index - 1) / 2;
            while (index > 0 && _comparer(_elements[index], _elements[parentIndex]) > 0)
            {
                (_elements[index], _elements[parentIndex]) = (_elements[parentIndex], _elements[index]);
                index = parentIndex;
                parentIndex = (index - 1) / 2;
            }
        }
        //
        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        // private void HeapifyDown(int index)
        // {
        //     int leftChildIndex = index * 2 + 1;
        //     int rightChildIndex = index * 2 + 2;
        //     int largestIndex = index;
        //
        //     if (leftChildIndex < _count && _comparer(_elements[leftChildIndex], _elements[largestIndex]) > 0)
        //     {
        //         largestIndex = leftChildIndex;
        //     }
        //
        //     if (rightChildIndex < _count && _comparer(_elements[rightChildIndex], _elements[largestIndex]) > 0)
        //     {
        //         largestIndex = rightChildIndex;
        //     }
        //
        //     if (largestIndex != index)
        //     {
        //         (_elements[index], _elements[largestIndex]) = (_elements[largestIndex], _elements[index]);
        //         HeapifyDown(largestIndex);
        //     }
        // }

        // 循环版本
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void HeapifyDown(int index)
        {
            // int maxIteration = (int)Math.Log(_count, 2) + 1;// 放置死循环 虽然不可能
            // while (maxIteration-- > 0)
            while (true)
            {
                int leftChildIndex = index * 2 + 1;
                int rightChildIndex = index * 2 + 2;
                int largestIndex = index;

                if (leftChildIndex < _count && _comparer(_elements[leftChildIndex], _elements[largestIndex]) > 0)
                {
                    largestIndex = leftChildIndex;
                }

                if (rightChildIndex < _count && _comparer(_elements[rightChildIndex], _elements[largestIndex]) > 0)
                {
                    largestIndex = rightChildIndex;
                }

                if (largestIndex == index)//OK
                {
                    break;
                }

                (_elements[index], _elements[largestIndex]) = (_elements[largestIndex], _elements[index]);
                index = largestIndex;
            }

            // if (maxIteration <= 0)
            // {
                // throw new Exception("HeapifyDown DeadLoop");
            // }
        }


        private void Grow()
        {
            TElement[] newElements = new TElement[_elements.Length * DefaultCapacityIncrease];
            Array.Copy(_elements, newElements, _elements.Length);
            _elements = newElements;
        }
    }
}