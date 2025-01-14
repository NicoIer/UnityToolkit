using System;
using System.Diagnostics;
using System.Threading;

namespace Network
{
    public struct TimeSample : IDisposable
    {
        // UnityEngine.Time isn't thread safe. use stopwatch instead.
        readonly Stopwatch watch;

        // remember when Begin was called
        double beginTime;

        // keep accumulating times over the given interval.
        // (not readonly. we modify its contents.)
        ExponentialMovingAverage ema;

        // average in seconds.
        // code often runs in sub-millisecond time. float is more precise.
        //
        // set with Interlocked for thread safety.
        // can be read from main thread while sampling happens in other thread.
        public double average; // THREAD SAFE

        // average over N begin/end captures
        public TimeSample(int n)
        {
            watch = new Stopwatch();
            watch.Start();
            ema = new ExponentialMovingAverage(n);
            beginTime = 0;
            average = 0;
        }

        // begin is called before the code to be sampled
        public void Begin()
        {
            // remember when Begin was called.
            // keep StopWatch running so we can average over the given interval.
            beginTime = watch.Elapsed.TotalSeconds;
            // Debug.Log($"Begin @ {beginTime:F4}");
        }

        // end is called after the code to be sampled
        public void End()
        {
            // add duration in seconds to accumulated durations
            double elapsed = watch.Elapsed.TotalSeconds - beginTime;
            ema.Add(elapsed);

            // expose new average thread safely
            Interlocked.Exchange(ref average, ema.Value);
        }

        public void Dispose()
        {
            watch.Stop();
        }
    }
}