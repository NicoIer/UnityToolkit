using System;
using System.Net;
using kcp2k;

namespace Nico
{
    /// <summary>
    /// Kcp的服务器端传输
    /// </summary>
    [Serializable]
    public class KcpServerTransport : ServerTransport
    {
        public ushort port { get; private set; }

        private readonly KcpConfig _config;


        private KcpServer _server;
        public int ConnectionCount => _server.connections.Count;

        public KcpServerTransport(KcpConfig config, ushort port)
        {
            this.port = port;
            this._config = config;
            _server = new KcpServer(
                (connectId) => onConnected?.Invoke(connectId),
                (connectId, data, channel) => onDataReceived?.Invoke(connectId, data, KcpUtil.FromKcpChannel(channel)),
                (connectionId) => onDisconnected?.Invoke(connectionId),
                (connectionId, error, msg) => onError?.Invoke(connectionId, KcpUtil.ToTransportError(error), msg),
                this._config
            );
        }


        public override Uri Uri()
        {
            UriBuilder builder = new UriBuilder();
            builder.Scheme = nameof(KcpServerTransport);
            builder.Host = System.Net.Dns.GetHostName();
            builder.Port = port;
            return builder.Uri;
        }

        public override bool Active() => _server.IsActive();


        public override void Start() => _server.Start(port);


        public override void Send(int connectionId, ArraySegment<byte> segment, int channelId = Channels.Reliable)
        {
            _server.Send(connectionId, segment, KcpUtil.ToKcpChannel(channelId));
            onDataSent?.Invoke(connectionId, segment, channelId);
        }

        public override void SendToAll(ArraySegment<byte> segment, int channelId = Channels.Reliable)
        {
            foreach (var conn in _server.connections)
            {
                _server.Send(conn.Key, segment, KcpUtil.ToKcpChannel(channelId));
                onDataSent?.Invoke(conn.Key, segment, channelId);
            }
        }

        public override void Disconnect(int connectionId) => _server.Disconnect(connectionId);

        public override string GetClientAddress(int connectionId)
        {
            IPEndPoint endPoint = _server.GetClientEndPoint(connectionId);
            if (endPoint != null)
            {
                if (endPoint.Address.IsIPv4MappedToIPv6)
                {
                    return endPoint.Address.MapToIPv4().ToString();
                }

                return endPoint.Address.ToString();
            }

            return "";
        }

        public override void Stop() => _server.Stop();

        public override int GetMaxPacketSize(int channelId = Channels.Reliable)
        {
            switch (channelId)
            {
                case Channels.Unreliable:
                    return KcpPeer.UnreliableMaxMessageSize(_config.Mtu);
                default:
                    return KcpPeer.ReliableMaxMessageSize(_config.Mtu, _config.ReceiveWindowSize);
            }
        }

        public override void TickOutgoing()
        {
            _server.TickOutgoing();
        }

        public override void TickIncoming()
        {
            _server.TickIncoming();
        }

        public override void Shutdown()
        {
            _server.Stop();
        }
    }
}