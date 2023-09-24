using System;
using System.Collections.Generic;

namespace UnityToolkit
{
    /// <summary>
    /// 类型注册容器
    /// </summary>
    public sealed class IOCContainer
    {
        private readonly Dictionary<Type, object> _mInstances = new Dictionary<Type, object>();

        public void Set<T>(T instance)
        {
            var key = typeof(T);
            _mInstances[key] = instance;
        }

        public T Get<T>()
        {
            var key = typeof(T);

            if (_mInstances.TryGetValue(key, out var retInstance))
            {
                return (T)retInstance;
            }

            return default(T);
        }
    }
}