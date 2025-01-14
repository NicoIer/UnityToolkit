using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
        }

        private readonly Timer _pushTimer;
        private readonly Timer _pingTimer;
        private readonly TimeSpan _pushInterval;
        private readonly TimeSpan _pingInterval;

        private readonly Dictionary<int, ClientTimeState> _rttDict;
        public IReadOnlyDictionary<int, ClientTimeState> RttDict => _rttDict;
        private readonly NetworkServerMessageHandler _messageHandler;
        private readonly IServerSocket _socket;

        public NetworkTimeServer(IServerSocket socket, TimeSpan pushInterval, TimeSpan pingInterval)
        {
            Debug.Assert(pingInterval < pushInterval);
            _socket = socket;
            _rttDict = new Dictionary<int, ClientTimeState>();

            _socket.OnConnected += OnConnected;
            _socket.OnDataReceived += OnData;
            _socket.OnDisconnected += OnDisconnected;
            _socket.OnStarted += OnStart;
            _socket.OnStopped += OnStop;

            _messageHandler = new NetworkServerMessageHandler();
            _messageHandler.Add<PongMessage>(OnPongMessage);

            _pushInterval = pushInterval;
            _pingInterval = pingInterval;
            _pushTimer = new Timer(OnPushTimer, null, Timeout.Infinite, Timeout.Infinite);
            _pingTimer = new Timer(OnPingTimer, null, Timeout.Infinite, Timeout.Infinite);
        }

        ~NetworkTimeServer()
        {
            _pushTimer.Dispose();
            _pingTimer.Dispose();

            _socket.OnConnected -= OnConnected;
            _socket.OnDataReceived -= OnData;
            _socket.OnDisconnected -= OnDisconnected;
            _socket.OnStarted -= OnStart;
            _socket.OnStopped -= OnStop;
        }

        private void OnPingTimer(object state)
        {
            if (_socket.ConnectionsCount == 0) return;
            NetworkBuffer payloadBuffer = NetworkBufferPool.Shared.Get();
            NetworkBuffer packetBuffer = NetworkBufferPool.Shared.Get();

            PingMessage msg = new PingMessage(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            NetworkPacker.Pack(msg, payloadBuffer, packetBuffer);

            _socket.SendToAll(packetBuffer);

            NetworkBufferPool.Shared.Return(payloadBuffer);
            NetworkBufferPool.Shared.Return(packetBuffer);
        }

        private void OnPongMessage(int connectionId, PongMessage pong)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long rtt = now - pong.sendTimeMs;
            lock (_rttDict)
            {
                _rttDict[connectionId].rtt.Add(rtt);
            }

            ToolkitLog.Debug($"NetworkTimeServer->{connectionId}'s RTT: {rtt}ms");
        }

        private void OnPushTimer(object state)
        {
            NetworkBuffer payloadBuffer = NetworkBufferPool.Shared.Get();
            NetworkBuffer packetBuffer = NetworkBufferPool.Shared.Get();

            long msNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            foreach (var (connectId, value) in _rttDict)
            {
                payloadBuffer.Reset();
                packetBuffer.Reset();
                int rttMs = (int)value.rtt.Value;
#if DEBUG
                ToolkitLog.Debug($"NetworkTimeServer->Push: {connectId}'s RTT: {rttMs}ms At {DateTimeOffset.FromUnixTimeMilliseconds(msNow):h:mm:ss tt zz}");
#endif
                ServerTimestampMessage msg = new ServerTimestampMessage(msNow, rttMs);
                NetworkPacker.Pack(msg, payloadBuffer, packetBuffer);
                _socket.Send(connectId, packetBuffer);
            }

            NetworkBufferPool.Shared.Return(payloadBuffer);
            NetworkBufferPool.Shared.Return(packetBuffer);
        }


        private void OnStart()
        {
            _pushTimer.Change(_pushInterval, _pushInterval);
            _pingTimer.Change(_pingInterval, _pingInterval);
        }


        private void OnStop()
        {
            _pushTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _pingTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        private void OnConnected(int connectionId)
        {
            ToolkitLog.Debug($"NetworkTimeServer->OnConnected: {connectionId}");
            _rttDict.Add(connectionId, new ClientTimeState());
        }

        private void OnDisconnected(int connectionId)
        {
            ToolkitLog.Debug($"NetworkTimeServer->OnDisconnected: {connectionId}");
            _rttDict.Remove(connectionId);
        }

        private void OnData(int connectionId, ArraySegment<byte> payload)
        {
            NetworkPacker.Unpack(payload, out NetworkPacket packet);
            _messageHandler.Handle(packet.id, connectionId, packet.payload);
        }
    }
}