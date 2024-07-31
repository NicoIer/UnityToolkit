using System;

namespace UnityToolkit
{
    public sealed class BindData<TData, TResult> : IDisposable
    {
        private event Func<TData, TResult> Listeners = delegate { return default; };
        private readonly TData _data;

        public BindData(TData data)
        {
            _data = data;
        }

        public TResult Invoke()
        {
            return Listeners(_data);
        }

        public ICommand Listen(Func<TData, TResult> action)
        {
            Listeners += action;
            return new CommonCommand(() => UnListen(action));
        }

        public void UnListen(Func<TData, TResult> action)
        {
            Listeners -= action;
        }

        public void Dispose()
        {
            Listeners = null;
        }
    }

    public sealed class BindData<T> : IDisposable
    {
        private event Action<T> Listeners = delegate { };
        private readonly T _data;

        public BindData(T data)
        {
            _data = data;
        }

        public void Invoke()
        {
            Listeners(_data);
        }

        public ICommand Listen(Action<T> action)
        {
            Listeners += action;
            return new CommonCommand(() => UnListen(action));
        }

        public void UnListen(Action<T> action)
        {
            Listeners -= action;
        }

        public void Dispose()
        {
            Listeners = null;
        }
    }
}