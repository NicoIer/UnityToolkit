using System.Collections.Generic;
using UnityEngine;

namespace Nico
{
    public enum TimerType
    {
        LoopForever,
        Delay,
    }

    public enum TimerState
    {
        Running,
        Paused,
        Stopped,
        OnGet,
    }

    // 基于MonoBehavior的Update驱动的定时器
    public static class TimerManager
    {
        [AddComponentMenu("")]
        internal class TimerBehavior : MonoBehaviour
        {
            private void LateUpdate() => Tick(Time.deltaTime);
        }

        private static GameObject _timerGameObject;
        private static Queue<Timer> _timers = new Queue<Timer>();
        private static readonly HashSet<Timer> _runningTimers = new HashSet<Timer>();

        // private static CancellationTokenSource _tickToken;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void CreateTimerGameObject()
        {
            // MonoBehavior Tick
            _timerGameObject = new GameObject("Timer")
            {
                hideFlags = HideFlags.HideInHierarchy
            };
            _timerGameObject.AddComponent<TimerBehavior>();
            UnityEngine.Object.DontDestroyOnLoad(_timerGameObject);

            Application.quitting -= ClearStatic;
            Application.quitting += ClearStatic;
        }

        private static void ClearStatic()
        {
            // _tickToken?.Cancel();
            UnityEngine.Object.DestroyImmediate(_timerGameObject); // MonoBehaviour Tick
            _timers.Clear();
            _runningTimers.Clear();
        }

        public static Timer Get(float duration, TimerType type)
        {
            if (_timers.Count == 0)
            {
                return new Timer(duration, type);
            }

            var timer = _timers.Dequeue();
            timer.duration = duration;
            timer.type = type;
            timer.state = TimerState.OnGet;
            return timer;
        }

        public static void Stop(Timer timer)
        {
            if (timer.state == TimerState.Stopped)
            {
                return;
            }

            timer.state = TimerState.Stopped;
            timer.onStopped?.Invoke();
            _runningTimers.Remove(timer);
            _timers.Enqueue(timer);
        }

        public static bool Start(Timer timer)
        {
            if (timer.state != TimerState.OnGet)
            {
                return false;
            }

            if (_runningTimers.Contains(timer))
            {
                return false;
            }

            timer.state = TimerState.Running;
            _runningTimers.Add(timer);
            return true;
        }


        public static void Pause(Timer timer)
        {
            timer.state = TimerState.Paused;
            timer.onPaused?.Invoke();
        }

        public static void Resume(Timer timer)
        {
            timer.state = TimerState.Running;
            timer.onResumed?.Invoke();
        }

        internal static void Tick(float deltaTime)
        {
            foreach (var runningTimer in _runningTimers)
            {
                if (runningTimer.Tick(deltaTime))
                {
                    Stop(runningTimer);
                }
            }
        }

        // internal static async UniTask TickTask(float deltaTime)
        // {
        //     while (!_tickToken.IsCancellationRequested)
        //     {
        //         await UniTask
        //     }
        // }
    }
}