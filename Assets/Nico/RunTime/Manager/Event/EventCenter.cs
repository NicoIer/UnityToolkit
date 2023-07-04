using System;
using System.Collections.Generic;
using System.Threading;

namespace Nico
{
    internal class EventCenter<TEvent> where TEvent: IEvent
    {
        private readonly HashSet<IEventListener<TEvent>> _listeners;
        // 之所以这里加锁 是因为 EventCenter 不一定只在主线程访问 它是 MonoBehavior无关的
        private readonly ReaderWriterLockSlim _lock;
        private bool _triggering;

        public EventCenter()
        {
            _listeners = new HashSet<IEventListener<TEvent>>();
            _lock = new ReaderWriterLockSlim();
        }

        public void AddListener(IEventListener<TEvent> listener)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_triggering)
                {
                    throw new ArgumentException(
                        "EventCenter is triggering, please don't add listener in event trigger");
                }
                _listeners.Add(listener);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RemoveListener(IEventListener<TEvent> listener)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_triggering)
                {
                    throw new ArgumentException(
                        "EventCenter is triggering, please don't add listener in event trigger");
                    // return;
                }
                _listeners.Remove(listener);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Trigger(TEvent e)
        {
            _lock.EnterReadLock();
            try
            {
                _triggering = true;
                foreach (var listener in _listeners)
                {
                    listener.OnReceiveEvent(e);
                }
            }
            finally
            {
                _lock.ExitReadLock();
                _triggering = false;
            }
        }
    }
}