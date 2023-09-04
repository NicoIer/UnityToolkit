using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nico
{
    /// <summary>
    /// 时间快照接口
    /// </summary>
    public interface ITimeSnapshot
    {
        public double remoteTime { get; set; } //远程时间戳
        public double localTime { get; set; } //本地时间戳
    }

    /// <summary>
    /// 时间快照插值
    /// </summary>
    public struct TimeSnapshotInterpolation
    {
        public double catchupRate; //追赶速度
        public double slowdownRate; //减速速度
        public double lower; //绝对追赶负阈值
        public double upper; //绝对追赶正阈值

        public TimeSnapshotInterpolation(double catchupRate, double slowdownRate,
            double lower, double upper)
        {
            this.catchupRate = catchupRate;
            this.slowdownRate = slowdownRate;
            this.lower = lower;
            this.upper = upper;
        }

        /// <summary>
        /// 计算在当前时间差下的时间缩放比例
        /// </summary>
        /// <param name="drift">时间差,绝对值</param>
        /// <returns></returns>
        public double Timescale(double drift)
        {
            if (drift > upper)
            {
                return 1 + catchupRate;
            }

            if (drift < lower)
            {
                return 1 - slowdownRate;
            }

            return 1;
        }
        //TODO 
        
    }

    /// <summary>
    /// 时间快照缓冲区
    /// </summary>
    /// <typeparam name="TTimeSnapshot">时间快照类型</typeparam>
    public class TimeSnapshotBuffer<TTimeSnapshot> : SortedList<double, TTimeSnapshot>
        where TTimeSnapshot : ITimeSnapshot
    {
        private uint _limit;

        public TimeSnapshotBuffer(uint limit)
        {
            this._limit = limit;
        }

        private void BtachRemove(int amount)
        {
            for (int i = 0; i < amount && i < Count; i++)
            {
                RemoveAt(0);
            }
        }

        public bool InsertIfNotExists(TTimeSnapshot snapshot)
        {
            if (Count >= _limit) return false;
            int before = Count;
            this[snapshot.remoteTime] = snapshot;
            return Count > before;
        }


        /// <summary>
        /// 查询给定时间戳 所在的buffer索引区间 以及比例
        /// </summary>
        /// <param name="timeLine"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="percent"></param>
        public void Query(double timeLine, out int from, out int to, out double percent)
        {
            from = to = -1;
            percent = 0;
            for (int i = 0; i < Count - 1; i++)
            {
                TTimeSnapshot start = this.Values[i];
                TTimeSnapshot end = this.Values[i + 1];
                if (start.remoteTime <= timeLine && timeLine <= end.remoteTime)
                {
                    from = i;
                    to = i + 1;

                    percent = (timeLine - start.remoteTime) / (end.remoteTime - start.remoteTime);
                    return;
                }
            }

            if (this.Values[0].remoteTime > timeLine)
            {
                from = to = 0;
                percent = 0;
                return;
            }

            from = to = Count - 1;
            percent = 0;
        }
    }
}