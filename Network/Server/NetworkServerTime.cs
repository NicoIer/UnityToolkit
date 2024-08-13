using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityToolkit;

namespace Network.Server
{
    public class NetworkServerTime : ISystem, IOnUpdate, IOnInit<NetworkServer>
    {
        //1毫秒==10,000个DateTime.Now.Ticks
        public const long DefaultPingInterval = 10000000; // 1s
        public const int PingWindowSize = 24; // average over 10 pings
        public static long LocalTimeTicks => DateTime.UtcNow.Ticks;
        private readonly IServerSocket _socket;
        private readonly NetworkBufferPool _bufferPool;
        private readonly Dictionary<int, PingPongRecord> _rttDict;
        // public ushort targetFrameRate;

        public NetworkServerTime(IServerSocket socket, NetworkBufferPool bufferPool) //, ushort targetFrameRate)
        {
            // this.targetFrameRate = targetFrameRate;
            _socket = socket;
            _bufferPool = bufferPool;
            _rttDict = new Dictionary<int, PingPongRecord>(16);
            socket.OnConnected += OnConnected;
            socket.OnDisconnected += OnDisconnected;
        }

        public void OnInit()
        {
        }

        public void OnInit(NetworkServer t)
        {
            t.Register<PongMessage>(OnReceivePong);
        }

        public void OnDisconnected(int connectionId)
        {
            if (_rttDict.ContainsKey(connectionId))
            {
                _rttDict.Remove(connectionId);
            }
        }

        public void OnConnected(int connectionId)
        {
            PingPongRecord pingPongRecord = new PingPongRecord
            {
                rtt = new ExponentialMovingAverage(PingWindowSize),
                lastReceivePongTime = 0,
                lastSendPingTime = 0,
                lastSendPongTime = 0,
            };
            _rttDict.Add(connectionId, pingPongRecord);
        }

        // 自己Ping了别人 别人回了Pong
        public void OnReceivePong(int connectionId, PongMessage pong)
        {
            PingPongRecord record = _rttDict[connectionId];
            if (pong.sendPingTimeTicks != record.lastSendPingTime)
            {
                NetworkLogger.Error(
                    $"Receive Pong message with wrong sendPingTime, connectionId: {connectionId}. Pong.sendPingTime: {pong.sendPingTimeTicks}, record.lastSendPingTime: {record.lastSendPingTime}");
                return;
            }

            // record.lastReceivePongTime = LocalTimeTicks;
            long newRttTicks = LocalTimeTicks - pong.sendPingTimeTicks; // 当前时间 距离 自己Ping别人的时间
            // 减去两个frame的时间
            // newRttTicks -= TimeSpan.FromSeconds(1d / targetFrameRate).Ticks * 2;
            record.rtt.Add(newRttTicks);
            var rtt = Rtt(connectionId);
            // NetworkLogger.Info($"client[{connectionId}],rtt:{rtt}-{rtt.Milliseconds}ms");
            RttMessage rttMessage = new RttMessage(rtt.Milliseconds);
            _socket.Send(connectionId, rttMessage, _bufferPool);
            record.pinging = false;
        }


        public void OnUpdate()
        {
            foreach (var (connectionId, record) in _rttDict)
            {
                if (record.pinging)
                {
                    continue;
                }

                if (LocalTimeTicks - record.lastSendPingTime > DefaultPingInterval)
                {
                    PingMessage pingMessage = new PingMessage(LocalTimeTicks);
                    _socket.Send(connectionId, pingMessage, _bufferPool);
                    record.lastSendPingTime = pingMessage.sendTimeTicks;
                    record.pinging = true;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TimeSpan Rtt(int connectionId)
        {
            // NetworkLogger.Warning(
            //     $"Cast: {_rttDict[connectionId].rtt.Value} ->{(long)_rttDict[connectionId].rtt.Value}");
            return TimeSpan.FromTicks((long)_rttDict[connectionId].rtt.Value);
        }

        public void Dispose()
        {
            _rttDict.Clear();
        }
    }
}