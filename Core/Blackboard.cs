using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityToolkit
{
    [Serializable]
    public sealed class Blackboard : IBlackboard
    {
        public event Action<string> OnRemove = delegate { };
        public event Action<string> OnAdd = delegate { };
        public event Action<string> OnSet = delegate { };

#if ODIN_INSPECTOR_3
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get<T>(in string key)
        {
            return (T)_data[key];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(in string key, out object o)
        {
            return _data.TryGetValue(key, out o);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue<T>(in string key, out T o)
        {
            if (_data.TryGetValue(key, out var obj))
            {
                o = (T)obj;
                return true;
            }

            o = default;
            return false;
        }

        public object this[string key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _data[key];
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(key, value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set<T>(in string key, T value)
        {
            if (!_data.TryAdd(key, value))
            {
                _data[key] = value;
                OnAdd(key);
                OnSet(key);
                return;
            }

            OnSet(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(in string key)
        {
            return _data.ContainsKey(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Remove(in string key)
        {
            OnRemove(key);
            _data.Remove(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            foreach (var key in _data.Keys)
            {
                OnRemove(key);
            }

            _data.Clear();
        }
    }
}