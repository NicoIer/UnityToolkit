using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityToolkit
{
    public class Pool<T>
    {
        private readonly Stack<T> _objects;

        // some types might need additional parameters in their constructor, so
        // we use a Func<T> generator
        readonly Func<T> _objectGenerator;

        public Pool(Func<T> objectGenerator, int initialCapacity)
        {
            this._objectGenerator = objectGenerator;
            _objects = new Stack<T>(initialCapacity);
            // allocate an initial pool so we have fewer (if any)
            // allocations in the first few frames (or seconds).
            for (int i = 0; i < initialCapacity; ++i)
                _objects.Push(objectGenerator());
        }

        // take an element from the pool, or create a new one if empty
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get() => _objects.Count > 0 ? _objects.Pop() : _objectGenerator();

        // return an element to the pool
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T item) => _objects.Push(item);

        // count to see how many objects are in the pool. useful for tests.
        public int Count => _objects.Count;
    }
}