using System;
using System.Collections.Generic;
using UnityEngine;


namespace Nico
{
    [Serializable]
    public class StreamingDict
    {
        [field: SerializeReference] private Dictionary<Type, Queue<object>> type2Queue { get; set; } = new();


        public void Push(params object[] objects)
        {
            foreach (var o in objects)
            {
                Push(o);
            }
        }

        public void Push(object obj)
        {
            var key = obj.GetType();
            // Debug.Log($"Push obj = {obj},Type:{key}");

            if (type2Queue.TryGetValue(key, out var value))
            {
                value.Enqueue(obj);
            }
            else
            {
                type2Queue[key] = new Queue<object>();
                type2Queue[key].Enqueue(obj);
            }
            // GetSize();
        }

        public bool Peek<T>(out T obj)
        {
            // GetSize();
            if (type2Queue.TryGetValue(typeof(T), out var queue) && queue.Count > 0)
            {
                obj = (T)queue.Peek();
                return true;
            }

            obj = default;
            return false;
        }

        public bool TryPop<T>(out T obj)
        {
            if (type2Queue.TryGetValue(typeof(T), out var queue) && queue.Count > 0)
            {
                obj = (T)queue.Dequeue();
                return true;
            }

            foreach (var kvp in type2Queue)
            {
                var type = kvp.Key;
                var link = kvp.Value;
                if (type != typeof(T) && !typeof(T).IsAssignableFrom(type)) continue;
                if (link.Count <= 0) continue;
                obj = (T)link.Dequeue();
                return true;
            }

            obj = default;
            return false;
        }

    }

}