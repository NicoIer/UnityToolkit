using System;
using System.Collections.Generic;
using Google.Protobuf;

namespace Nico
{
    public class EventCenter
    {
        private interface ICenter
        {
        }

        private class Center<T> : ICenter
        {
            public Action<T> onEvent;
        }

        private readonly Dictionary<Type, ICenter> _centers;

        public EventCenter()
        {
            _centers = new Dictionary<Type, ICenter>();
        }

        public void Listen<T>(Action<T> action)
        {
            if (!_centers.TryGetValue(typeof(T), out ICenter center))
            {
                center = new Center<T>();
                _centers.Add(typeof(T), center);
            }

            (center as Center<T>).onEvent -= action;
            (center as Center<T>).onEvent += action;
        }

        public void UnListen<T>(Action<T> action)
        {
            if (!_centers.TryGetValue(typeof(T), out ICenter center))
            {
                return;
            }

            (center as Center<T>).onEvent -= action;
        }

        public void Fire<T>(T t)
        {
            if (!_centers.TryGetValue(typeof(T), out ICenter center))
            {
                return;
            }

            (center as Center<T>).onEvent?.Invoke(t);
        }
    }
}