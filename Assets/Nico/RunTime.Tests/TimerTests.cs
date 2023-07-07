using System;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Nico.Tests
{
    public class TimerTests : MonoBehaviour
    {
        public async void Start()
        {
            var timer = TimerManager.Get(2, TimerType.Delay);
            Assert.AreEqual(TimerState.OnGet, timer.state);
            timer.onCompleted += () => { Debug.Log("Timer Completed"); };
            timer.onPaused += () => { Debug.Log("Timer Paused"); };
            timer.onResumed += () => { Debug.Log("Timer Resumed"); };
            timer.onStopped += () => { Debug.Log("Timer Stopped"); };
            
            TimerManager.Start(timer);
            // Assert.AreEqual(TimerState.Running, timer.state);
            await UniTask.Delay(100);
            TimerManager.Pause(timer);
            await UniTask.Delay(100);
            TimerManager.Resume(timer);
            await UniTask.Delay(100);
            TimerManager.Stop(timer);
            //等待3秒后输出Timer Completed
            // Assert.AreEqual(TimerState.Stopped, timer.state);
        }
    }
}