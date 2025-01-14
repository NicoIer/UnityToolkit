using System;
using kcp2k;

namespace Network.Client
{
    public class KcpClientSocket : IClientSocket
    {
        public bool connecting => !connected && _client.active;
        public bool connected => _client.connected;
        public event Action OnConnected = delegate { };
        public event Action<ArraySegment<byte>> OnDataReceived = delegate { };
        public event Action OnDisconnected = delegate { };
        public event Action<ArraySegment<byte>> OnDataSent = delegate { };
        private readonly KcpChannel _channel;
        private readonly KcpClient _client;

        public KcpClientSocket(KcpConfig config,KcpChannel channel)
        {
            _channel = channel;
            _client = new KcpClient(() => { OnConnected(); }
                , (message, _) => { OnDataReceived(message); }
                , () => { OnDisconnected(); }
                , (error, reason) => { NetworkLogger.Error($"[KCP] OnServerError({error}, {reason}"); }
                , config
            );
        }

        public void TickIncoming()
        {
            _client.TickIncoming();
        }

        public void TickOutgoing()
        {
            _client.TickOutgoing();
        }

        public void Connect(Uri uri)
        {
            _client.Connect(uri.Host, (ushort)uri.Port);
        }

        public void Send(ArraySegment<byte> data)
        {
            _client.Send(data,_channel);
        }

        public void Disconnect()
        {
            _client.Disconnect();
        }
    }
}