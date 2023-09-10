using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Nico
{
    public class ConcurrentPool<T>
    {
        readonly ConcurrentStack<T> objects = new ConcurrentStack<T>();
        readonly System.Func<T> objectGenerator;

        public ConcurrentPool(System.Func<T> objectGenerator, int initialCapacity)
        {
            this.objectGenerator = objectGenerator;
            for (int i = 0; i < initialCapacity; ++i)
                objects.Push(objectGenerator());
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get()
        {
            return objects.TryPop(out var item) ? item : objectGenerator();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T item)
        {
            objects.Push(item);
        }

        public int Count => objects.Count;
    }
}