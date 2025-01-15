using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Network.Client;
using Network.Server;
using Network.Telepathy;
using UnityToolkit;

namespace Network
{
    /// <summary>
    /// 网络对时服务器
    /// </summary>
    public sealed class NetworkTimeServer
    {
        public class ClientTimeState
        {
            public ExponentialMovingAverage rtt;
            public ClientTimeState(int n)
            {
                rtt = new ExponentialMovingAverage(n);
            }
        }
        public const int DefaultEmaWindow = 10;

        private readonly TimeSpan _pushInterval;
        private readonly TimeSpan _pingInterval;

        private readonly Dictionary<int, ClientTimeState> _rttDict;
        public IReadOnlyDictionary<int, ClientTimeState> RttDict => _rttDict;
        private readonly NetworkServer _server;
        private readonly ICommand _removeOnPongMessage;

        private float _currentPingTimer;
        private float _currentPushTimer;

        public NetworkTimeServer(NetworkServer server,
            TimeSpan pushInterval, TimeSpan pingInterval)
        {
            Debug.Assert(pingInterval < pushInterval);

            _server = server;
            _rttDict = new Dictionary<int, ClientTimeState>();

            _server.OnUpdateEvent += OnUpdate;
            _server.socket.OnConnected += OnConnected;
            _server.socket.OnDisconnected += OnDisconnected;
            _server.socket.OnStarted += OnStart;
            _server.socket.OnStopped += OnStop;

            _removeOnPongMessage = server.messageHandler.Add<PongMessage>(OnPongMessage);

            _pushInterval = pushInterval;
            _pingInterval = pingInterval;
        }


        ~NetworkTimeServer()
        {
            _removeOnPongMessage.Execute();

            _server.OnUpdateEvent -= OnUpdate;
            _server.socket.OnConnected -= OnConnected;
            _server.socket.OnDisconnected -= OnDisconnected;
            _server.socket.OnStarted -= OnStart;
            _server.socket.OnStopped -= OnStop;
        }


        private void OnUpdate(in float deltaTime)
        {
            _currentPingTimer += deltaTime;
            _currentPushTimer += deltaTime;

            if (_currentPingTimer >= _pingInterval.TotalSeconds)
            {
                _currentPingTimer = 0;
                OnPingTimer();
            }

            if (_currentPushTimer >= _pushInterval.TotalSeconds)
            {
                _currentPushTimer = 0;
                OnPushTimer();
            }
        }

        private void OnPingTimer()
        {
            if (_server.socket.ConnectionsCount == 0) return;
            _server.SendToAll(new PingMessage(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        }

        private void OnPushTimer()
        {
            long msNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            foreach (var (connectId, value) in _rttDict)
            {
                int rttMs = (int)value.rtt.Value;
                // ToolkitLog.Info(
                // $"OnPushTimer: {connectId} rtt: {rttMs} now: {DateTimeOffset.FromUnixTimeMilliseconds(msNow):hh:mm:ss t z}");
                _server.Send(connectId, new ServerTimestampMessage(msNow, rttMs));
            }
        }

        private void OnPongMessage(int connectionId, PongMessage pong)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            long rtt = now - pong.sendTimeMs;
            _rttDict[connectionId].rtt.Add(rtt);
        }


        private void OnStart()
        {
        }


        private void OnStop()
        {
        }
        private void OnConnected(int connectionId)
        {
            _rttDict.Add(connectionId, new ClientTimeState(DefaultEmaWindow));
            long frameMaxTime = TimeSpan.FromSeconds(1d / _server.TargetFrameRate).Milliseconds; // 一帧最大时间
            _rttDict[connectionId].rtt.Add(frameMaxTime);
            PingTarget(connectionId);
        }

        private void PingTarget(int connectionId)
        {
            _server.Send(connectionId, new PingMessage(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
        }

        private void OnDisconnected(int connectionId)
        {
            _rttDict.Remove(connectionId);
        }
    }
}