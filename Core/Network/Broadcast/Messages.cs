// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
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