using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nico
{
    public class MaxHeap<TElement>
    {
        private const int DefaultCapacity = 4;
        private const int DefaultCapacityIncrease = 2;

        private readonly Func<TElement, TElement, int> _comparer;
        internal TElement[] elements { get; private set; }

        private int _count;
        public int Count => _count;
        public int Capacity => elements.Length;

        public MaxHeap(Func<TElement, TElement, int> comparer)
        {
            if (comparer == null)
            {
                throw new ArgumentException($"Heap<{typeof(TElement)}>.comparer is null");
            }

            this._comparer = comparer;
            elements = new TElement[DefaultCapacity];
            _count = 0;
        }

        public void Insert(TElement element)
        {
            if (_count == elements.Length)
            {
                Grow();
            }

            elements[_count] = element;
            HeapifyUp(_count);
            ++_count;
        }

        public TElement Peek()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Heap is empty");
            }

            return elements[0];
        }

        public TElement Pop()
        {
            if (_count == 0)
            {
                throw new InvalidOperationException("Heap is empty");
            }

            TElement result = elements[0];
            --_count;
            elements[0] = elements[_count];
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
            while (index > 0 && _comparer(elements[index], elements[parentIndex]) > 0)
            {
                (elements[index], elements[parentIndex]) = (elements[parentIndex], elements[index]);
                index = parentIndex;
                parentIndex = (index - 1) / 2;
            }
        }
        
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

                if (leftChildIndex < _count && _comparer(elements[leftChildIndex], elements[largestIndex]) > 0)
                {
                    largestIndex = leftChildIndex;
                }

                if (rightChildIndex < _count && _comparer(elements[rightChildIndex], elements[largestIndex]) > 0)
                {
                    largestIndex = rightChildIndex;
                }

                if (largestIndex == index)//OK
                {
                    break;
                }

                (elements[index], elements[largestIndex]) = (elements[largestIndex], elements[index]);
                index = largestIndex;
            }

            // if (maxIteration <= 0)
            // {
                // throw new Exception("HeapifyDown DeadLoop");
            // }
        }


        private void Grow()
        {
            TElement[] newElements = new TElement[elements.Length * DefaultCapacityIncrease];
            Array.Copy(elements, newElements, elements.Length);
            elements = newElements;
        }
        

    }
}