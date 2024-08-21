using System;
using Network.Telepathy;

namespace Network.Client
{
    public class TelepathyClientSocket : IClientSocket
    {
        public bool noDelay = true;

        public int sendTimeout = 5000;
        public int receiveTimeout = 30000;

        public int maxMessageSize = 16 * 1024;

        public int maxReceivesPerTick = 1000;

        public int sendQueueLimit = 10000;

        public int receiveQueueLimit = 10000;

        public Uri remoteUri;

        private TelepathyClient _client;

        private readonly Func<bool> _enabledCheck = () => true;

        public bool connecting
        {
            get { return _client != null && _client.Connecting; }
        }

        public bool connected
        {
            get { return _client != null && _client.Connected; }
        }

        public event Action OnConnected = delegate { };
        public event Action<ArraySegment<byte>> OnDataReceived = delegate { };
        public event Action OnDisconnected = delegate { };
        public event Action<ArraySegment<byte>> OnDataSent = delegate { };


        public void TickIncoming()
        {
            _client.Tick(maxReceivesPerTick, _enabledCheck);
        }

        public void TickOutgoing()
        {
        }


        public void Connect(Uri uri)
        {
            remoteUri = uri;
            _client = new TelepathyClient(maxMessageSize);
            // client hooks
            // other systems hook into transport events in OnCreate or
            // OnStartRunning in no particular order. the only way to avoid
            // race conditions where telepathy uses OnConnected before another
            // system's hook (e.g. statistics OnData) was added is to wrap
            // them all in a lambda and always call the latest hook.
            // (= lazy call)
            _client.OnConnected = () => OnConnected();
            _client.OnData = (segment) => OnDataReceived(segment);
            // fix: https://github.com/vis2k/Mirror/issues/3287
            // Telepathy may call OnDisconnected twice.
            // Mirror may have cleared the callback already, so use "?." here.
            _client.OnDisconnected = () => OnDisconnected();

            // client configuration
            _client.NoDelay = noDelay;
            _client.SendTimeout = sendTimeout;
            _client.ReceiveTimeout = receiveTimeout;
            _client.SendQueueLimit = sendQueueLimit;
            _client.ReceiveQueueLimit = receiveQueueLimit;

            _client.Connect(uri.Host, uri.Port);
        }

        public void Send(ArraySegment<byte> data)
        {
            _client.Send(data);
            OnDataSent(data);
        }

        public void Disconnect()
        {
            _client.Disconnect();
            _client = null;
            OnDisconnected();
        }
    }
}