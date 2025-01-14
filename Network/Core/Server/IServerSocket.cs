using System;
using System.Collections.Generic;

namespace Network.Server
{
    public interface IServerSocket
    {
        public event Action OnStarted;
        public event Action OnStopped;
        public event Action<int> OnConnected;
        public event Action<int, ArraySegment<byte>> OnDataReceived;
        public event Action<int> OnDisconnected;
        public event Action<int, ArraySegment<byte>> OnDataSent;

        public event Action<ArraySegment<byte>> OnDataSentToAll;
        
        public int ConnectionsCount { get; }
        public IEnumerable<int> GetConnections();

        public void Start();
        public void Send(int connectionId, ArraySegment<byte> segment);
        void SendToAll(ArraySegment<byte> segment);
        public void Disconnect(int connectionId);
        public string GetClientAddress(int connectionId);
        public void Stop();
        public Uri Uri();

        public void TickIncoming();
        public void TickOutgoing();
    }
}