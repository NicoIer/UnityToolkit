using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Nico
{
    public class ConcurrentPool<T>
    {
        private readonly ConcurrentStack<T> _objects;
        private readonly System.Func<T> _objectGenerator;

        public ConcurrentPool(System.Func<T> objectGenerator, int initialCapacity)
        {
            _objects = new ConcurrentStack<T>();
            _objectGenerator = objectGenerator;
            for (int i = 0; i < initialCapacity; ++i)
                _objects.Push(objectGenerator());
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get()
        {
            return _objects.TryPop(out var item) ? item : _objectGenerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T item)
        {
            _objects.Push(item);
        }

        public int Count => _objects.Count;
    }
}