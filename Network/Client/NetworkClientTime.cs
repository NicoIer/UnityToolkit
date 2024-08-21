using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityToolkit;

namespace Network.Client
{
    public class NetworkClientTime : ISystem, IOnInit<NetworkClient>
    {
        /// <summary>
        /// 客户端的本地时间
        /// </summary>
        public static long LocalTimeTicks => DateTime.UtcNow.Ticks;


        public long RttTicks
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rtt * TimeSpan.TicksPerMillisecond;
        }
        public int Rtt
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _rtt;
        }

        private NetworkQuality _quality;
        public event Action<NetworkQuality> OnQualityChanged;

        /// <summary>
        /// ms RTT是服务器给的 服务器会做插值 所以客户端不做插值
        /// </summary>
        private int _rtt;

        private NetworkClient _client;


        private List<ICommand> _disposeList;

        public NetworkClientTime()
        {
            OnQualityChanged = delegate { };
            _disposeList = new List<ICommand>();
        }

        public void OnInit(NetworkClient t)
        {
            _client = t;
            _disposeList.Add(t.AddMsgHandler<PingMessage>(OnReceivePing));
            _disposeList.Add(t.AddMsgHandler<RttMessage>(OnReceiveRtt));
        }

        /// <summary>
        /// 客户端收到服务器Ping消息时调用,立刻回复Pong消息
        /// </summary>
        /// <param name="pingMessage"></param>
        private void OnReceivePing(PingMessage pingMessage)
        {
            PongMessage pongMessage = new PongMessage(ref pingMessage);
            _client.Send(pongMessage);
        }


        private void OnReceiveRtt(RttMessage obj)
        {
            _rtt = obj.rttMs;
            NetworkQuality quality = Utils.CalculateQuality(obj.rttMs);

            if (_quality != quality)
            {
                _quality = quality;
                OnQualityChanged(_quality);
            }

            // TimeSpan span = TimeSpan.FromMilliseconds(obj.rttMs);
            // NetworkLogger.Info($"rtt: {span}-{span.Milliseconds}ms , quality: {_quality}");
        }

        public void Dispose()
        {
            foreach (var command in _disposeList)
            {
                command.Execute();
            }

            OnQualityChanged = delegate { };
            _disposeList.Clear();
        }
    }
}