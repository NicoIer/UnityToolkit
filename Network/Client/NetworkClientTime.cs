using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityToolkit;

namespace Network.Client
{
    public class NetworkClientTime : ISystem, IOnInit<NetworkClient>, IOnUpdate
    {
        /// <summary>
        /// 服务器的当前时间
        /// </summary>
        public long ServerTimeTicks => _stopwatch.ElapsedTicks + _lastServerTimeTicks;
        private long _lastServerTimeTicks;

        /// <summary>
        /// 客户端发送的消息 到达服务器的时间
        /// </summary>
        public long MsgArriveTimeTicks
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ServerTimeTicks + RttTicks / 2;
        }

        /// <summary>
        /// 客户端发送Ping消息的间隔
        /// </summary>
        public const long PingIntervalTicks = 5000 * TimeSpan.TicksPerMillisecond;

        private long _lastPingTimeTicks = long.MinValue;

        public long RttTicks
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (long)_rttEma.Value;
        }

        public int RttMs
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)(_rttEma.Value / TimeSpan.TicksPerMillisecond);
        }

        public double RttSeconds
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rttEma.Value / TimeSpan.TicksPerSecond;
        }

        public NetworkQuality Quality
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _quality;
        }

        private NetworkQuality _quality;
        public event Action<NetworkQuality> OnQualityChanged;
        public const int PingWindowSize = 24; // average over 10 pings
        
        private ExponentialMovingAverage _rttEma = new ExponentialMovingAverage(PingWindowSize);

        private NetworkClient _client;


        private readonly List<ICommand> _disposeList;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public NetworkClientTime()
        {
            OnQualityChanged = delegate { };
            _disposeList = new List<ICommand>(3);
        }

        public void OnInit(NetworkClient t)
        {
            _client = t;
            _disposeList.Add(t.AddMsgHandler<PingMessage>(OnReceivePing));
            _disposeList.Add(t.AddMsgHandler<PongMessage>(OnReceivePong));
            _disposeList.Add(t.AddMsgHandler<TimestampMessage>(OnTimestamp));
        }


        public void Dispose()
        {
            foreach (var command in _disposeList)
            {
                command.Execute();
            }

            OnQualityChanged = delegate { };
            _disposeList.Clear();
            _stopwatch.Stop();
        }

        public void OnUpdate(float deltaTime)
        {
            if (ServerTimeTicks - _lastPingTimeTicks > PingIntervalTicks)
            {
                _lastPingTimeTicks = ServerTimeTicks;
                PingMessage ping = new PingMessage();
                _client.Send(ping, true);
            }
        }

        /// <summary>
        /// 客户端收到服务器Ping消息时调用,立刻回复Pong消息
        /// </summary>
        /// <param name="pingMessage"></param>
        private void OnReceivePing(PingMessage pingMessage)
        {
            PongMessage pongMessage = new PongMessage(ref pingMessage);
            _client.Send(pongMessage, true);
        }


        private void OnReceivePong(PongMessage pong)
        {
            long rttTick = (ServerTimeTicks - pong.sendTimeTicks);
            _rttEma.Add(rttTick);
            int rttMs = (int)(_rttEma.Value / TimeSpan.TicksPerMillisecond);
            NetworkQuality quality = Utils.CalculateQuality(rttMs);

            if (_quality != quality)
            {
                _quality = quality;
                Debug.Assert(OnQualityChanged != null, nameof(OnQualityChanged) + " != null");
                OnQualityChanged(_quality);
            }
        }

        private void OnTimestamp(TimestampMessage timestamp)
        {
            _lastServerTimeTicks = timestamp.timestampTicks;
            _stopwatch.Restart();
        }
    }
}