using System;
using System.Collections.Generic;

namespace UnityToolkit
{
    public interface ISystemLocator : IDisposable
    {
        public void Register<T>() where T : ISystem, new();

        public void UnRegister<T>() where T : ISystem;

        public T Get<T>() where T : ISystem;
    }

    public interface ISystem : IDisposable
    {
        void OnInit();
    }
    
    
    public class SystemLocator : ISystemLocator
    {
        public Dictionary<Type, ISystem> Systems { get; private set; } = new Dictionary<Type, ISystem>();

        public void Register<T>() where T : ISystem, new()
        {
            if (Systems.ContainsKey(typeof(T)))
            {
                return;
            }

            T system = new T();
            Systems.Add(typeof(T), system);
            system.OnInit();
        }

        public void UnRegister<T>() where T : ISystem
        {
            if (Systems.TryGetValue(typeof(T), out ISystem system))
            {
                system.Dispose();
            }

            Systems.Remove(typeof(T));
        }

        public T Get<T>() where T : ISystem
        {
            if (Systems.TryGetValue(typeof(T), out ISystem system) && system is T tSystem)
            {
                return tSystem;
            }
            return default;
        }

        public void Dispose()
        {
            foreach (var system in Systems.Values)
            {
                system.Dispose();
            }
            Systems.Clear();
        }
    }
}