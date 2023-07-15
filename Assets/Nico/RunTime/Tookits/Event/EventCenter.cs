using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Nico
{
    internal class EventCenter<TEvent> where TEvent : IEvent
    {
        private readonly HashSet<IEventListener<TEvent>> _listeners;

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
                throw new ArgumentException(
                    "EventCenter is triggering, please don't add listener in event trigger");
            }

            _listeners.Add(listener);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveListener(IEventListener<TEvent> listener)
        {
            if (_triggering)
            {
                throw new ArgumentException(
                    "EventCenter is triggering, please don't add listener in event trigger");
                // return;
            }

            _listeners.Remove(listener);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Trigger(TEvent e)
        {
            _triggering = true;
            foreach (var listener in _listeners)
            {
                listener.OnReceiveEvent(e);
            }

            _triggering = false;
        }
    }
}