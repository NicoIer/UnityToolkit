using System;

namespace Nico
{
    [Serializable]
    public abstract class ServerTransport : INetTransport
    {
        // 与客户端连接时触发
        public Action<int> OnConnected;

        // 接收到客户端消息时触发
        public Action<int, ArraySegment<byte>, int> OnDataReceived;

        // 发送消息到客户端时触发
        public Action<int, ArraySegment<byte>, int> OnDataSent;

        // 发生错误时触发
        public Action<int, TransportError, string> OnError;

        // 与客户端断开连接时触发
        public Action<int> OnDisconnected;

        //  是否可用
        public abstract Uri Uri();

        //  是否可用
        public abstract bool Active();

        //  开始监听
        public abstract void Start();

        // 发送消息到客户端
        public abstract void Send(int connectionId, ArraySegment<byte> segment, int channelId = Channels.Reliable);
    
        public abstract void SendToAll(ArraySegment<byte> segment, int channelId = Channels.Reliable);
        
        
        //  断开连接
        public abstract void Disconnect(int connectionId);

        // 获取客户端地址
        public abstract string GetClientAddress(int connectionId);

        // 停止监听
        public abstract void Stop();

        // 关闭        
        public abstract int GetMaxPacketSize(int channelId = Channels.Reliable);
        
        public abstract void Shutdown();
        
        public abstract void TickOutgoing();
        
        public abstract void TickIncoming();
    }
}