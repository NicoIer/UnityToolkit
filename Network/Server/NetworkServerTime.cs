using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityToolkit;

namespace Network.Server
{
    public class NetworkServerTime : ISystem, IOnUpdate, IOnInit<NetworkServer>
    {
        private class Record
        {
            public ExponentialMovingAverage rtt;
            public bool pinging = false; // 是否正在Ping
            public long lastReceivePingTime => lastSendPongTime; // 上一次收到Ping消息的时间
            public long lastReceivePongTime; // 上一次收到Pong消息的时间
            public long lastSendPingTime; // 上一次发送Ping消息的时间
            public long lastSendPongTime; // 上一次发送Pong消息的时间

            public long lastSendTimestampTime; // 上一次发送Timestamp消息的时间

            public Record()
            {
                rtt = new ExponentialMovingAverage(PingWindowSize);
                lastReceivePongTime = 0;
                lastSendPingTime = 0;
                lastSendPongTime = 0;
                lastSendTimestampTime = 0;
            }
        }

        /// <summary>
        /// 服务器主动Ping客户端的间隔 用于测量RTT
        /// </summary>
        public const long DefaultPingInterval = 1 * TimeSpan.TicksPerSecond; // 1s

        /// <summary>
        /// 服务器发送对时消息的间隔 用于同步时间
        /// </summary>
        public const long DefaultSyncTimeInterval = 15 * TimeSpan.TicksPerSecond; // 15s

        public const int PingWindowSize = 24; // average over 10 pings
        private static long LocalTimeTicks => DateTime.UtcNow.Ticks;
        private NetworkServer _server;

        private readonly Dictionary<int, Record> _rttDict;

        // public ushort targetFrameRate;
        private ICommand _removeMsgHandlerCommand;
        private ICommand _removeMsgHandlerCommand2;

        public NetworkServerTime()
        {
            _rttDict = new Dictionary<int, Record>(16);
        }

        public void OnInit(NetworkServer t)
        {
            _server = t;
            _server.socket.OnConnected += OnConnected;
            _server.socket.OnDisconnected += OnDisconnected;
            _removeMsgHandlerCommand = _server.AddMsgHandler<PongMessage>(OnReceivePong);
            _removeMsgHandlerCommand2 = _server.AddMsgHandler<PingMessage>(OnReceivePing);
        }

        public void Dispose()
        {
            _rttDict.Clear();
            _server.socket.OnConnected -= OnConnected;
            _server.socket.OnDisconnected -= OnDisconnected;
            _removeMsgHandlerCommand?.Execute();
            _removeMsgHandlerCommand2?.Execute();
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
            Record record = new Record();
            _rttDict.Add(connectionId, record);

            // 一旦连接上了，就给对方发送一个时间戳
            TimestampMessage timestampMessage = new TimestampMessage(LocalTimeTicks);
            _server.Send(connectionId, timestampMessage);
            record.lastSendTimestampTime = timestampMessage.timestampTicks;
        }

        // 别人Ping了我
        private void OnReceivePing(int connectionId, PingMessage ping)
        {
            PongMessage pong = new PongMessage(ref ping);
            _server.Send(connectionId, pong);
        }

        // 自己Ping了别人 别人回了Pong
        private void OnReceivePong(int connectionId, PongMessage pong)
        {
            Record record = _rttDict[connectionId];
            if (pong.sendTimeTicks != record.lastSendPingTime)
            {
                NetworkLogger.Error(
                    $"Receive Pong message with wrong sendPingTime, connectionId: {connectionId}. Pong.sendPingTime: {pong.sendTimeTicks}, record.lastSendPingTime: {record.lastSendPingTime}");
                return;
            }

            long newRttTicks = LocalTimeTicks - pong.sendTimeTicks; // 当前时间 距离 自己Ping别人的时间
            record.rtt.Add(newRttTicks);
            record.lastReceivePongTime = LocalTimeTicks;
            record.pinging = false;
        }


        public void OnUpdate()
        {
            foreach (var (connectionId, record) in _rttDict)
            {
                if (!record.pinging && LocalTimeTicks - record.lastSendPingTime > DefaultPingInterval)
                {
                    PingMessage pingMessage = new PingMessage(LocalTimeTicks);
                    _server.Send(connectionId, pingMessage);
                    record.lastSendPingTime = pingMessage.sendTimeTicks;
                    record.pinging = true;
                }

                if (record.lastSendTimestampTime == 0 ||
                    LocalTimeTicks - record.lastSendTimestampTime > DefaultSyncTimeInterval)
                {
                    TimestampMessage timestampMessage = new TimestampMessage(LocalTimeTicks);
                    _server.Send(connectionId, timestampMessage);
                    record.lastSendTimestampTime = timestampMessage.timestampTicks;
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