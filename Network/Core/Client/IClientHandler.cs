using System;

namespace Network.Client
{
    public interface IClientHandler
    {
        void OnConnected();
        void OnDataReceived(ArraySegment<byte> data);
        void OnDisconnected();
        void OnDataSent(ArraySegment<byte> data);
        
        void OnUpdate();
    }
}