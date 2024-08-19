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
    }

    public interface IOnUpdate
    {
        void OnUpdate();
    }

    public interface IOnInit<T>
    {
        void OnInit(T t);
    }

    public interface IOnInit
    {
        void OnInit();
    }

    public class SystemLocator : ISystemLocator
    {
        protected Dictionary<int, ISystem> _systems;
        public Dictionary<int, ISystem>.ValueCollection systems => _systems.Values;

        public SystemLocator()
        {
            _systems = new Dictionary<int, ISystem>();
        }

        public virtual void Register<T>(T system) where T : ISystem
        {
            _systems.Add(TypeId<T>.stableId, system);
            if (system is IOnInit initSystem)
                initSystem.OnInit();
        }

        public virtual void Register<T>() where T : ISystem, new()
        {
            if (_systems.ContainsKey(TypeId<T>.stableId))
            {
                return;
            }

            T system = new T();
            Register(system);
        }

        public virtual void UnRegister<T>() where T : ISystem
        {
            if (_systems.TryGetValue(TypeId<T>.stableId, out ISystem system))
            {
                system.Dispose();
            }

            _systems.Remove(TypeId<T>.stableId);
        }

        public virtual T Get<T>() where T : ISystem
        {
            if (_systems.TryGetValue(TypeId<T>.stableId, out ISystem system) && system is T tSystem)
            {
                return tSystem;
            }

            return default;
        }

        public virtual void Dispose()
        {
            foreach (var system in _systems.Values)
            {
                system.Dispose();
            }

            _systems.Clear();
        }
    }
}