using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace UnityToolkit.Collections
{
    /// <summary>
    /// 一次遍历多个List
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct ListEnumerator<T> : IEnumerator<T>
    {
        private readonly T[] items;
        public int length { get; private set; }

        private int position;

        public ListEnumerator(params IList<T>[] lists)
        {
            length = 0;
            for (int i = 0; i < lists.Length; i++)
            {
                length += lists[i].Count;
            }

            items = ArrayPool<T>.Shared.Rent(length);
            int index = 0;
            for (int i = 0; i < lists.Length; i++)
            {
                for (int j = 0; j < lists[i].Count; j++)
                {
                    items[index++] = lists[i][j];
                }
            }

            position = -1;
            Current = default;
        }

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(items);
        }

        public bool MoveNext()
        {
            position++;
            if (position < length)
            {
                Current = items[position];
                return true;
            }

            return false;
        }

        public void Reset()
        {
            position = -1;
            Current = default;
        }

        public T Current { get; private set; }

        object IEnumerator.Current => Current;
    }
}