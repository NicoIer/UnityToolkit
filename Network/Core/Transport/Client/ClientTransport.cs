using System;

namespace Nico
{
    public abstract class ClientTransport : INetTransport
    {
        // 连接到服务器 时触发
        public Action onConnected;

        // 与服务器断开连接时触发
        public Action onDisconnected;

        // 接收到服务器消息时触发
        public Action<ArraySegment<byte>, int> onDataReceived;

        // 发送消息到服务器时触发
        public Action<ArraySegment<byte>, int> onDataSent;

        // 发生错误时触发
        public Action<TransportError, string> onError;

        public abstract bool connected { get; }
        
        // 连接到服务器
        public abstract void Connect(string address);

        // 发送消息到服务器
        public abstract void Send(ArraySegment<byte> segment, int channelId = Channels.Reliable);

        // 断开连接
        public abstract void Disconnect();

        // 关闭
        public abstract int GetMaxPacketSize(int channelId = Channels.Reliable);

        public abstract void Shutdown();
        
        public abstract void TickOutgoing();
        
        public abstract void TickIncoming();
    }
}