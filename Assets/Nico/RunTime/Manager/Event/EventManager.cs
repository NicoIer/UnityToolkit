using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nico
{
    internal static class EventCenters<T> where T : IEvent
    {
        internal static EventCenter<T> center;
    }
    
    
    public static class EventManager
    {
        //由于使用了泛型 因此 Editor注册的事件 进入 PlayerMode无法清空 可能会导致异常  所以 限制在PlayerMode下使用
        public static void Listen<TEvent>(IEventListener<TEvent> listener) where TEvent : IEvent
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("EventCenter should not be used in edit mode");
                return;
            }

            if (EventCenters<TEvent>.center == null)
            {
                EventCenters<TEvent>.center = new EventCenter<TEvent>();
                Application.quitting -= ClearEventCenter<TEvent>;
                Application.quitting += ClearEventCenter<TEvent>;
            }

            EventCenters<TEvent>.center.AddListener(listener);
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ClearEventCenter<T>() where T : IEvent
        {
            // Debug.Log($"ClearEventCenter<{typeof(T)}>");
            EventCenters<T>.center = null;
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UnListen<TEvent>(IEventListener<TEvent> listener) where TEvent : IEvent
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("EventCenter should not be used in edit mode");
                return;
            }

            if (EventCenters<TEvent>.center == null)
            {
                Debug.LogWarning($"EventCenter<{typeof(TEvent)}> not exist");
                return;
            }

            EventCenters<TEvent>.center.RemoveListener(listener);
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Trigger<TEvent>() where TEvent : struct, IEvent
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("EventCenter should not be used in edit mode");
                return;
            }

            Trigger<TEvent>(default(TEvent));
        }

        // [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Trigger<TEvent>(TEvent e) where TEvent : IEvent
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("EventCenter should not be used in edit mode");
                return;
            }

            if (EventCenters<TEvent>.center == null)
            {
                Debug.LogWarning($"EventCenter<{typeof(TEvent)}> not exist");
                return;
            }

            EventCenters<TEvent>.center.Trigger(e);
        }
    }
}