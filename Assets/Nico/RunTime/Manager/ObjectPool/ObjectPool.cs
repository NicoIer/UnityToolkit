using System.Collections.Generic;

namespace Nico
{
    internal static class ObjectPool<T> where T : IPoolObject, new()
    {
        private static readonly Queue<T> _objects = new Queue<T>();

        internal static T Get()
        {
            if (_objects.Count == 0)
            {
                var t = new T();
                t.OnSpawn();
                t.state = PoolObjectState.Spawned;
                return t;
            }

            var obj= _objects.Dequeue();
            obj.OnSpawn();
            obj.state = PoolObjectState.Spawned;
            return obj;
        }

        internal static void Return(T obj)
        {
            obj.OnRecycle();
            obj.state = PoolObjectState.Recycled;
            _objects.Enqueue(obj);
        }
    }
}