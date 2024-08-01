using System;

namespace Network.Server
{
    public interface IServerSocket
    {
        public event Action<int> OnConnected;
        public event Action<int, ArraySegment<byte>> OnDataReceived;
        public event Action<int> OnDisconnected;
        public event Action<int, ArraySegment<byte>> OnDataSent;

        public void Start();
        public void Send(int connectionId, ArraySegment<byte> segment);
        public void Disconnect(int connectionId);
        public string GetClientAddress(int connectionId);
        public void Stop();
        public Uri Uri();

        public void TickIncoming();
        public void TickOutgoing();
    }
}