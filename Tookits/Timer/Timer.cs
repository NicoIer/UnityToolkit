using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nico
{
    public sealed class Timer
    {
        internal float duration { get; set; }
        public float leftTime { get; internal set; }
        public TimerType type { get; internal set; }
        public TimerState state { get; internal set; }

        public Action onCompleted;
        public Action onPaused;
        public Action onResumed;
        public Action onStopped;

        internal Timer(float duration, TimerType type)
        {
            this.duration = duration;
            leftTime = duration;
            this.type = type;
            this.state = TimerState.OnGet;
        }

        // 返回值代表是否已经结束了 不需要再执行了
        internal bool Tick(float deltaTime)
        {
            
            //非运行也非暂停状态下 不需要再执行了 通知移除
            if (state != TimerState.Running && state != TimerState.Paused)
            {
                return true;
            }
            // Debug.Log("Timer Tick");
            leftTime -= deltaTime;
            if (leftTime > 0)
            {
                return false;
            }
            switch (type)
            {
                case TimerType.LoopForever:
                    leftTime = duration;
                    onCompleted?.Invoke();
                    return false;
                case TimerType.Delay:
                    onCompleted?.Invoke();
                    return true;
            }
            //未知类型 通知移除
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start()
        {
            TimerManager.Start(this);
        }
    }
}