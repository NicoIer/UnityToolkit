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

        public void Attach(Action unRegister)
        {
            this._unRegister += unRegister;
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

    public delegate void EventHandler<TEvent>(in TEvent args);

    public delegate void EventHandler<TEvent1, TEvent2>(in TEvent1 args1, in TEvent2 args2);

    public delegate void EventHandler<TEvent1, TEvent2, TEvent3>(in TEvent1 args1, in TEvent2 args2, in TEvent3 args3);

    public delegate void EventHandler<TEvent1, TEvent2, TEvent3, TEvent4>(in TEvent1 args1, in TEvent2 args2,
        in TEvent3 args3, in TEvent4 args4);

    public delegate void EventHandler<TEvent1, TEvent2, TEvent3, TEvent4, TEvent5>(in TEvent1 args1, in TEvent2 args2,
        in TEvent3 args3, in TEvent4 args4, in TEvent5 args5);

    public delegate TResult ResultEventHandler<TParam, out TResult>(in TParam param);


    /// <summary>
    /// 基于类型的事件系统 静态全局唯一的 性能更好 基于TypeHash
    /// </summary>
    public static class StaticEventSystem
    {
        private static class EventCache<T>
        {
            public static readonly BuildInEvent<T> item = new BuildInEvent<T>();
        }

        private static class ResultEventCache<TParam, TResult>
        {
            public static readonly BuildInResultEvent<TParam, TResult> item = new BuildInResultEvent<TParam, TResult>();
        }

        public static void Invoke<T>() where T : new()
        {
            EventCache<T>.item.Invoke(new T());
        }

        public static void Invoke<T>(T args)
        {
            EventCache<T>.item.Invoke(args);
        }

        public static ICommand Listen<T>(EventHandler<T> onEvent)
        {
            return EventCache<T>.item.Register(onEvent);
        }

        public static void UnListen<T>(EventHandler<T> onEvent)
        {
            EventCache<T>.item.UnRegister(onEvent);
        }

        public static ICommand Listen<T, TResult>(ResultEventHandler<T, TResult> onResultEvent)
        {
            return ResultEventCache<T, TResult>.item.Register(onResultEvent);
        }

        public static void UnListen<T, TResult>(ResultEventHandler<T, TResult> onResultEvent)
        {
            ResultEventCache<T, TResult>.item.UnRegister(onResultEvent);
        }

        public static TResult Invoke<T, TResult>(T args)
        {
            return ResultEventCache<T, TResult>.item.Invoke(args);
        }
    }

    /// <summary>
    /// 基于类型的事件系统
    /// </summary>
    public sealed class TypeEventSystem
    {
        // 提供一个全局的事件系统便于使用
        public static readonly TypeEventSystem Global = new TypeEventSystem();

        private readonly EventRepository _repository = new EventRepository();


        public void Invoke<T>() where T : new()
        {
            // EventCache<T>.item.Invoke(new T());
            _repository.Get<BuildInEvent<T>>()?.Invoke(new T());
        }

        public void Invoke<T>(T args)
        {
            // EventCache<T>.item.Invoke(args);
            _repository.Get<BuildInEvent<T>>()?.Invoke(args);
        }


        public ICommand Listen<T>(EventHandler<T> onEvent)
        {
            return _repository.GetOrAdd<BuildInEvent<T>>().Register(onEvent);
        }

        public void UnListen<T>(EventHandler<T> onEvent)
        {
            _repository.Get<BuildInEvent<T>>()?.UnRegister(onEvent);
        }

        public ICommand Listen<T, TResult>(ResultEventHandler<T, TResult> onResultEvent)
        {
            return _repository.GetOrAdd<BuildInResultEvent<T, TResult>>().Register(onResultEvent);
        }

        public void UnListen<T, TResult>(ResultEventHandler<T, TResult> onResultEvent)
        {
            _repository.Get<BuildInResultEvent<T, TResult>>()?.UnRegister(onResultEvent);
        }

        public TResult Invoke<T, TResult>(T args)
        {
            return _repository.Get<BuildInResultEvent<T, TResult>>().Invoke(args);
        }
    }

    /// <summary>
    /// 事件仓库 用于存储事件
    /// </summary>
    internal class EventRepository
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
    public interface IOnEvent<TType>
    {
        void OnEvent(in TType args);
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
        private event ResultEventHandler<TParam, TResult> _onEvent = delegate { return default; };

        public ICommand Register(ResultEventHandler<TParam, TResult> onResultEvent)
        {
            this._onEvent += onResultEvent;
            return new CommonCommand(() => { UnRegister(onResultEvent); });
        }

        public void UnRegister(ResultEventHandler<TParam, TResult> onResultEvent)
        {
            this._onEvent -= onResultEvent;
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

        public void Invoke()
        {
            _onEvent();
        }
    }

    public sealed class BuildInEvent<T> : IEvent
    {
        private EventHandler<T> _onEvent;

        public ICommand Register(EventHandler<T> onEvent)
        {
            this._onEvent += onEvent;
            return new CommonCommand(() => { UnRegister(onEvent); });
        }

        public void UnRegister(EventHandler<T> onEvent)
        {
            this._onEvent -= onEvent;
        }

        public void Invoke(in T args)
        {
            _onEvent(in args);
        }
    }

    public sealed class BuildInEvent<T1, T2> : IEvent
    {
        private EventHandler<T1, T2> _onEvent = delegate { };

        public ICommand Register(EventHandler<T1, T2> onEvent)
        {
            this._onEvent += onEvent;
            return new CommonCommand(() => { UnRegister(onEvent); });
        }

        public void UnRegister(EventHandler<T1, T2> onEvent)
        {
            this._onEvent -= onEvent;
        }

        public void Invoke(in T1 args1, in T2 args2)
        {
            _onEvent(in args1, in args2);
        }
    }

    public sealed class BuildInEvent<T1, T2, T3> : IEvent
    {
        private EventHandler<T1, T2, T3> _onEvent = delegate { };

        public ICommand Register(EventHandler<T1, T2, T3> onEvent)
        {
            this._onEvent += onEvent;
            return new CommonCommand(() => { UnRegister(onEvent); });
        }

        public void UnRegister(EventHandler<T1, T2, T3> onEvent)
        {
            this._onEvent -= onEvent;
        }

        public void Invoke(in T1 args1, in T2 args2, in T3 args3)
        {
            _onEvent(in args1, in args2, in args3);
        }
    }

    public sealed class BuildInEvent<T1, T2, T3, T4> : IEvent
    {
        private EventHandler<T1, T2, T3, T4> _onEvent = delegate { };

        public ICommand Register(EventHandler<T1, T2, T3, T4> onEvent)
        {
            this._onEvent += onEvent;
            return new CommonCommand(() => { UnRegister(onEvent); });
        }

        public void UnRegister(EventHandler<T1, T2, T3, T4> onEvent)
        {
            this._onEvent -= onEvent;
        }

        public void Invoke(in T1 args1, in T2 args2, in T3 args3, in T4 args4)
        {
            _onEvent(in args1, in args2, in args3, in args4);
        }
    }

    public sealed class BuildInEvent<T1, T2, T3, T4, T5> : IEvent
    {
        private EventHandler<T1, T2, T3, T4, T5> _onEvent = delegate { };

        public ICommand Register(EventHandler<T1, T2, T3, T4, T5> onEvent)
        {
            this._onEvent += onEvent;
            return new CommonCommand(() => { UnRegister(onEvent); });
        }

        public void UnRegister(EventHandler<T1, T2, T3, T4, T5> onEvent)
        {
            this._onEvent -= onEvent;
        }

        public void Invoke(in T1 args1, in T2 args2, in T3 args3, in T4 args4, in T5 args5)
        {
            _onEvent(in args1, in args2, in args3, in args4, in args5);
        }
    }
}