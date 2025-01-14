using System;
using MemoryPack;

namespace Network.Broadcast
{
    [MemoryPackable]
    public partial struct BroadcastMessage : INetworkMessage
    {
        public int senderId;
        public ArraySegment<byte> payload;
    }

    [MemoryPackable]
    public partial struct BroadcastTargetedMessage : INetworkMessage
    {
        public int senderId;
        public ArraySegment<int> targetIds;
        public ArraySegment<byte> payload;
    }


    [MemoryPackable]
    public partial struct BroadcastNearbyMessage : INetworkMessage
    {
        public int senderId;
        public ArraySegment<byte> payload;
    }
}