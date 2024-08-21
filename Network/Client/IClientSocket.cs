using System;

namespace Network.Client
{
    public interface IClientSocket
    {
        public bool connecting { get; }
        public bool connected { get; }
        public event Action OnConnected;
        public event Action<ArraySegment<byte>> OnDataReceived;
        public event Action OnDisconnected;
        public event Action<ArraySegment<byte>> OnDataSent;

        public void TickIncoming();
        public void TickOutgoing();

        public void Connect(Uri uri);

        public void Send(ArraySegment<byte> data);
        public void Disconnect();
    }
}