using System;
using kcp2k;

namespace Nico
{
    public class KcpClientTransport : ClientTransport
    {
        public ushort port;
        private readonly KcpConfig _config;
        private KcpClient _client;

        public KcpClientTransport(KcpConfig config, ushort port)
        {
            _config = config;
            this.port = port;
            _client = new KcpClient(
                () => onConnected.Invoke(),
                (data, channel) => onDataReceived.Invoke(data, KcpUtil.FromKcpChannel(channel)),
                () => onDisconnected.Invoke(),
                (error, msg) => onError.Invoke(KcpUtil.ToTransportError(error), msg),
                config
            );
        }

        public override bool connected => _client.connected;

        public override void Connect(string address) => _client.Connect(address, port);


        public override void Send(ArraySegment<byte> segment, int channelId = Channels.Reliable)
        {
            _client.Send(segment, KcpUtil.ToKcpChannel(channelId));
            onDataSent?.Invoke(segment, channelId);
        }

        public override void Disconnect() => _client.Disconnect();

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
            _client.TickOutgoing();
        }

        public override void TickIncoming()
        {
            _client.TickIncoming();
        }

        public override void Shutdown()
        {
            _client.Disconnect();
        }
    }
}