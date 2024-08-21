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
        private NetworkServer _server;

        private readonly Dictionary<int, PingPongRecord> _rttDict;

        // public ushort targetFrameRate;
        private ICommand _removeMsgHandlerCommand;

        public NetworkServerTime()
        {
            _rttDict = new Dictionary<int, PingPongRecord>(16);
        }

        public void OnInit(NetworkServer t)
        {
            _server = t;
            _server.socket.OnConnected += OnConnected;
            _server.socket.OnDisconnected += OnDisconnected;
            _removeMsgHandlerCommand = _server.AddMsgHandler<PongMessage>(OnReceivePong);
        }

        public void Dispose()
        {
            _rttDict.Clear();
            _server.socket.OnConnected -= OnConnected;
            _server.socket.OnDisconnected -= OnDisconnected;
            _removeMsgHandlerCommand?.Execute();
            _server = null;
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
            _server.Send(connectionId, rttMessage);
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
                    _server.Send(connectionId, pingMessage);
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
    }
}