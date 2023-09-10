using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
//TODO 把Unity和C# 分开 这样就可以在非Unity中使用
namespace Nico
{
    internal class EventCenter<TEvent> where TEvent : IEvent
    {
        private readonly HashSet<IEventListener<TEvent>> _listeners;
        private readonly List<IEventListener<TEvent>> _removeList = new List<IEventListener<TEvent>>();

        private readonly List<IEventListener<TEvent>> _addList = new List<IEventListener<TEvent>>();

        // 之所以这里加锁 是因为 EventCenter 不一定只在主线程访问 它是 MonoBehavior无关的
        private bool _triggering;

        public EventCenter()
        {
            _listeners = new HashSet<IEventListener<TEvent>>();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void AddListener(IEventListener<TEvent> listener)
        {
            if (_triggering)
            {
                _addList.Add(listener);
                return;
            }

            _listeners.Add(listener);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveListener(IEventListener<TEvent> listener)
        {
            if (_triggering)
            {
                _removeList.Add(listener);
                return;
            }

            _listeners.Remove(listener);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Trigger(TEvent e)
        {
            _triggering = true;
            foreach (var listener in _listeners)
            {
                if (listener == null)
                {
                    continue;
                }

                listener.OnReceiveEvent(e);
            }

            _triggering = false;
            if(_removeList.Count > 0)
            {
                foreach (var listener in _removeList)
                {
                    _listeners.Remove(listener);
                }
                _removeList.Clear();
            }
            
            if(_addList.Count > 0)
            {
                foreach (var listener in _addList)
                {
                    _listeners.Add(listener);
                }
                _addList.Clear();
            }
        }
    }
}