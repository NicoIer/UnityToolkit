using System;
using System.Collections.Generic;
using System.Net;
using kcp2k;

namespace Network.Server
{
    public class KcpServerSocket : IServerSocket
    {
        public const string Scheme = "kcp";
        public event Action OnStarted = delegate { };
        public event Action OnStopped = delegate { };
        public event Action<int> OnConnected = delegate { };
        public event Action<int, ArraySegment<byte>> OnDataReceived = delegate { };
        public event Action<int> OnDisconnected = delegate { };
        public event Action<int, ArraySegment<byte>> OnDataSent = delegate { };
        public event Action<ArraySegment<byte>> OnDataSentToAll = delegate { };
        private KcpServer _server;
        private ushort _port;
        private KcpChannel _channel;

        public KcpServerSocket(KcpConfig config, ushort port, KcpChannel channel)
        {
            _channel = channel;
            _port = port;
            _server = new KcpServer(
                connectionId => OnConnected(connectionId),
                (connectionId, message, _) => OnDataReceived(connectionId, message),
                connectionId => OnDisconnected(connectionId),
                (connectionId, error, reason) =>
                {
                    NetworkLogger.Error($"[KCP] OnServerError({connectionId}, {error}, {reason}");
                },
                config
            );
        }

        public int ConnectionsCount => _server.connections.Count;
        
        public IEnumerable<int> GetConnections()
        {
            return _server.connections.Keys;
        }

        public void Start()
        {
            _server.Start(_port);
            OnStarted();
        }

        public void Send(int connectionId, ArraySegment<byte> segment)
        {
            _server.Send(connectionId, segment, _channel);
            OnDataSent(connectionId, segment);
        }

        public void SendToAll(ArraySegment<byte> segment)
        {
            foreach (var connection in _server.connections.Keys)
            {
                _server.Send(connection, segment, _channel);
            }
            OnDataSentToAll(segment);
        }

        public void Disconnect(int connectionId)
        {
            _server.Disconnect(connectionId);
        }

        public string GetClientAddress(int connectionId)
        {
            return _server.connections[connectionId].remoteEndPoint.ToString();
        }

        public void Stop()
        {
            _server.Stop();
            OnStopped();
        }

        public Uri Uri()
        {
            UriBuilder uriBuilder = new UriBuilder
            {
                Scheme = Scheme,
                Host = Dns.GetHostName(),
                Port = _port
            };
            return uriBuilder.Uri;
        }

        public void TickIncoming()
        {
            _server.TickIncoming();
        }

        public void TickOutgoing()
        {
            _server.TickOutgoing();
        }
    }
}