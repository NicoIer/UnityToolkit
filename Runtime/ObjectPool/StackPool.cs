#if UNITY_5_6_OR_NEWER
using System.Collections.Generic;
using UnityEngine.Pool;

namespace UnityToolkit
{
    public static class StackPool<T>
    {
        private static readonly ObjectPool<Stack<T>> s_pool =
            new ObjectPool<Stack<T>>(() => new Stack<T>(), null, s => s.Clear());

        public static Stack<T> Get()
        {
            return s_pool.Get();
        }

        public static void Release(Stack<T> stack)
        {
            s_pool.Release(stack);
        }
    }
}
#endif