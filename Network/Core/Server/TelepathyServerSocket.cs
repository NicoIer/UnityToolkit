using System;
using System.Collections.Generic;
using System.Net;
using Network.Telepathy;

namespace Network.Server
{
    public class TelepathyServerSocket : IServerSocket
    {
        public const string Scheme = "tcp4";

        public ushort port = 7777;

        public bool noDelay = true;

        public int sendTimeout = 5000;
        public int receiveTimeout = 30000;

        public int maxMessageSize = 16 * 1024;
        public int maxReceivesPerTick = 10000;

        public int sendQueueLimitPerConnection = 10000;
        public int receiveQueueLimitPerConnection = 10000;

        public event Action OnStarted = delegate { };
        public event Action OnStopped = delegate { };
        public event Action<int> OnConnected = delegate { };
        public event Action<int, ArraySegment<byte>> OnDataReceived = delegate { };
        public event Action<int> OnDisconnected = delegate { };
        public event Action<int, ArraySegment<byte>> OnDataSent = delegate { };

        public event Action<ArraySegment<byte>> OnDataSentToAll = delegate { };

        private TelepathyServer _server = null;
        public EndPoint LocalEndPoint => _server.listener.LocalEndpoint;
        private Func<bool> _enabledCheck = () => true;

        public int ConnectionsCount => _server.clients.Count;

        public IEnumerable<int> GetConnections()
        {
            return _server.clients.Keys;
        }

        public void Start()
        {
            _server = new TelepathyServer(maxMessageSize);

            _server.OnConnected = (connectionId) => OnConnected(connectionId);
            _server.OnData = (connectionId, segment) => OnDataReceived(connectionId, segment);
            _server.OnDisconnected = (connectionId) => OnDisconnected(connectionId);

            // server configuration
            _server.NoDelay = noDelay;
            _server.SendTimeout = sendTimeout;
            _server.ReceiveTimeout = receiveTimeout;
            _server.SendQueueLimit = sendQueueLimitPerConnection;
            _server.ReceiveQueueLimit = receiveQueueLimitPerConnection;

            if (_server.Start(port))
            {
                OnStarted();
            }
        }

        public void Send(int connectionId, ArraySegment<byte> segment)
        {
            _server.Send(connectionId, segment);
            OnDataSent(connectionId, segment);
        }

        public void SendToAll(ArraySegment<byte> segment)
        {
            foreach (var connectionId in _server.clients.Keys)
            {
                Send(connectionId, segment);
            }

            OnDataSentToAll(segment);
        }

        public void Disconnect(int connectionId)
        {
            _server.Disconnect(connectionId);
        }

        public string GetClientAddress(int connectionId)
        {
            return _server.GetClientAddress(connectionId);
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
                Port = port
            };
            return uriBuilder.Uri;
        }

        public void TickIncoming()
        {
        }

        public void TickOutgoing()
        {
            _server.Tick(maxReceivesPerTick, _enabledCheck);
        }
    }
}