using System;

namespace Network.Server
{
    public interface IServerHandler
    {
        void OnConnected(int connectionId);
        void OnDataReceived(int connectionId, ArraySegment<byte> data);
        void OnDisconnected(int connectionId);
        void OnDataSent(int connectionId, ArraySegment<byte> data);

        void OnUpdate(float deltaTime);
    }
}