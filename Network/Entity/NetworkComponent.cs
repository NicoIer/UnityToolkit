using System;
using MemoryPack;

namespace Network
{
    public abstract class NetworkComponent
    {
        public abstract void FromPacket(in NetworkComponentPacket packet);

        public abstract NetworkComponentPacket ToDummyPacket(NetworkBuffer buffer);
    }

    
    /// <summary>
    /// NetworkComponent对应的网络包
    /// 通过type找到原始类型进行反序列化
    /// </summary>
    [MemoryPackable]
    public partial struct NetworkComponentPacket : INetworkMessage
    {
        public uint? entityId;
        public int? idx;
        public ushort? type;
        public ArraySegment<byte> data;
    }
}