using System;
using System.Collections.Generic;

namespace UnityToolkit
{
    /// <summary>
    /// 用于取消注册事件
    /// </summary>
    public struct CommonCommand : ICommand
    {
        private Action _unRegister;

        public CommonCommand(Action unRegister)
        {
            this._unRegister = unRegister;
        }

        public void Execute()
        {
            this._unRegister();
            _unRegister = null;
        }
    }
    //
    // public class KeyEventSystem
    // {
    //     public void Send(object key, object args)
    //     {
    //     }
    //
    //     public void Listen(object key, Action<object> onEvent)
    //     {
    //     }
    // }

    /// <summary>
    /// 基于类型的事件系统
    /// </summary>
    public class TypeEventSystem
    {
        // 提供一个全局的事件系统便于使用
        public static readonly TypeEventSystem Global = new TypeEventSystem();

        private readonly EventRepository _repository = new EventRepository();

        public void Send<T>() where T : new()
        {
            _repository.Get<BuildInEvent<T>>()?.Trigger(new T());
        }

        public void Send<T>(T args)
        {
            _repository.Get<BuildInEvent<T>>()?.Trigger(args);
        }

        public ICommand Listen<T>(Action<T> onEvent)
        {
            return _repository.GetOrAdd<BuildInEvent<T>>().Regiser(onEvent);
        }

        public void UnListen<T>(Action<T> onEvent)
        {
            _repository.Get<BuildInEvent<T>>()?.UnRegister(onEvent);
        }

        public ICommand Listen<T, TResult>(Func<T, TResult> onEvent)
        {
            return _repository.GetOrAdd<BuildInResultEvent<T, TResult>>().Register(onEvent);
        }

        public void UnListen<T, TResult>(Func<T, TResult> onEvent)
        {
            _repository.Get<BuildInResultEvent<T, TResult>>()?.UnRegister(onEvent);
        }

        public TResult SendWithResult<T, TResult>(T args)
        {
            return _repository.Get<BuildInResultEvent<T, TResult>>().Invoke(args);
        }
    }

    /// <summary>
    /// 事件仓库 用于存储事件
    /// </summary>
    public class EventRepository
    {
        private readonly Dictionary<Type, IEvent> _typeEvents = new Dictionary<Type, IEvent>();

        public void Add<T>() where T : IEvent, new()
        {
            _typeEvents.Add(typeof(T), new T());
        }

        public T Get<T>() where T : IEvent
        {
            IEvent e;
            if (_typeEvents.TryGetValue(typeof(T), out e))
            {
                return (T)e;
            }

            return default;
        }

        public T GetOrAdd<T>() where T : IEvent, new()
        {
            Type type = typeof(T);
            IEvent e;
            if (_typeEvents.TryGetValue(type, out e))
            {
                return (T)e;
            }

            e = new T();
            _typeEvents.Add(type, e);
            return (T)e;
        }
    }


    /// <summary>
    /// 事件监听者接口
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    public interface IOnEvent<in TType>
    {
        void OnEvent(TType args);
    }

    /// <summary>
    /// 事件接口
    /// </summary>
    public interface IEvent
    {
    }

    /// <summary>
    /// 提供给需要返回值的事件 通常用于Task等待
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public sealed class BuildInResultEvent<TParam, TResult> : IEvent
    {
        private event Func<TParam, TResult> _onEvent = _ => default;

        public ICommand Register(Func<TParam, TResult> onEvent)
        {
            this._onEvent += onEvent;
            return new CommonCommand(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Func<TParam, TResult> onEvent)
        {
            this._onEvent -= onEvent;
        }

        public TResult Invoke(TParam args)
        {
            return _onEvent(args);
        }
    }

    /// <summary>
    /// 事件定义,范型参数为事件参数类型
    /// </summary>
    public sealed class BuildInEvent : IEvent
    {
        private Action _onEvent = () => { };

        public ICommand Register(Action onEvent)
        {
            this._onEvent += onEvent;
            return new CommonCommand(() => { Unregister(onEvent); });
        }

        public void Unregister(Action onEvent)
        {
            this._onEvent -= onEvent;
        }

        public void Trigger()
        {
            _onEvent();
        }
    }

    public abstract class Event<T> : IEvent
    {
        private Action<T> _onEvent = _ => { };

        public ICommand Register(Action<T> onEvent)
        {
            this._onEvent += onEvent;
            return new CommonCommand(() => { Unregister(onEvent); });
        }

        public void Unregister(Action<T> onEvent)
        {
            this._onEvent -= onEvent;
        }

        public void Trigger(T args)
        {
            _onEvent(args);
        }
    }

    public sealed class BuildInEvent<T> : IEvent
    {
        private Action<T> _onEvent = _ => { };

        public ICommand Regiser(Action<T> onEvent)
        {
            this._onEvent += onEvent;
            return new CommonCommand(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T> onEvent)
        {
            this._onEvent -= onEvent;
        }

        public void Trigger(T args)
        {
            _onEvent(args);
        }
    }

    public sealed class BuildInEvent<T1, T2> : IEvent
    {
        private Action<T1, T2> _onEvent = (_, __) => { };

        public ICommand Register(Action<T1, T2> onEvent)
        {
            this._onEvent += onEvent;
            return new CommonCommand(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T1, T2> onEvent)
        {
            this._onEvent -= onEvent;
        }

        public void Trigger(T1 args1, T2 args2)
        {
            _onEvent(args1, args2);
        }
    }

    public sealed class BuildInEvent<T1, T2, T3> : IEvent
    {
        private Action<T1, T2, T3> _onEvent = (_, __, ___) => { };

        public ICommand Register(Action<T1, T2, T3> onEvent)
        {
            this._onEvent += onEvent;
            return new CommonCommand(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T1, T2, T3> onEvent)
        {
            this._onEvent -= onEvent;
        }
    }

    public sealed class BuildInEvent<T1, T2, T3, T4> : IEvent
    {
        private Action<T1, T2, T3, T4> _onEvent = (_, __, ___, ____) => { };

        public ICommand Register(Action<T1, T2, T3, T4> onEvent)
        {
            this._onEvent += onEvent;
            return new CommonCommand(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T1, T2, T3, T4> onEvent)
        {
            this._onEvent -= onEvent;
        }
    }

    public sealed class BuildInEvent<T1, T2, T3, T4, T5> : IEvent
    {
        private Action<T1, T2, T3, T4, T5> _onEvent = (_, __, ___, ____, _____) => { };

        public ICommand Register(Action<T1, T2, T3, T4, T5> onEvent)
        {
            this._onEvent += onEvent;
            return new CommonCommand(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T1, T2, T3, T4, T5> onEvent)
        {
            this._onEvent -= onEvent;
        }
    }
}