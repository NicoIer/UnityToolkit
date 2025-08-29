using MemoryPack;

namespace Network
{

    [MemoryPackable]
    public partial struct HeartBeat : INetworkMessage
    {
        public static readonly HeartBeat Default = new HeartBeat();
    }
    // /// <summary>
    // /// 服务器为客户端分配的连接ID
    // /// </summary>
    // [MemoryPackable]
    // public partial struct AssignConnectionIdMessage : INetworkMessage
    // {
    //     public readonly int id;
    //
    //     public AssignConnectionIdMessage(int id)
    //     {
    //         this.id = id;
    //     }
    // }
    //
    // /// <summary>
    // /// Ping消息
    // /// </summary>
    // [MemoryPackable]
    // public partial struct PingMessage : INetworkMessage
    // {
    //     public readonly long sendTimeMs;
    //
    //     public PingMessage(long sendTimeMs)
    //     {
    //         this.sendTimeMs = sendTimeMs;
    //     }
    // }
    //
    // [MemoryPackable]
    // public partial struct PongMessage : INetworkMessage
    // {
    //     /// <summary>
    //     /// Ping方发送Ping的时间
    //     /// Pinger -> Ponger -> Pinger
    //     /// </summary>
    //     public readonly long sendTimeMs;
    //
    //     public PongMessage(ref PingMessage pingMessage)
    //     {
    //         sendTimeMs = pingMessage.sendTimeMs;
    //     }
    // }
}