using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

    public interface ITaskSystem : ISystem
    {
        public Task task { get; }
        public Task Run();
    }

    public interface IOnUpdate
    {
        void OnUpdate(float deltaTime);
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

#if UNITY_EDITOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public Dictionary<int, ISystem>.ValueCollection systems => _systems.Values;

        private bool _disposing;

        public SystemLocator()
        {
            _systems = new Dictionary<int, ISystem>();
            _disposing = false;
        }

        public virtual void Register<T>(T system) where T : ISystem
        {
            if(_disposing)return;
            _systems.Add(TypeId<T>.stableId, system);
            if (system is IOnInit initSystem)
                initSystem.OnInit();
        }

        public virtual void Register<T>() where T : ISystem, new()
        {
            
            if(_disposing)return;
            if (_systems.ContainsKey(TypeId<T>.stableId))
            {
                return;
            }

            T system = new T();
            Register(system);
        }

        public virtual void UnRegister<T>() where T : ISystem
        {
            if(_disposing)return;
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
            _disposing = true;
            foreach (var system in _systems.Values)
            {
                system.Dispose();
            }

            _systems.Clear();
        }
    }
}