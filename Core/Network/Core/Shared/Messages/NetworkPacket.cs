// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using System;
using MemoryPack;

namespace Network
{
    /// <summary>
    /// 网络数据包
    /// </summary>
    [MemoryPackable]
    internal partial struct NetworkPacket : INetworkMessage
    {
        /// <summary>
        /// 携带数据的类型id
        /// </summary>
        public readonly ushort id;

        /// <summary>
        /// 携带数据的对应的二进制数据
        /// </summary>
        public ArraySegment<byte> payload;

        public NetworkPacket(ushort id, ArraySegment<byte> payload)
        {
            this.id = id;
            this.payload = payload;
        }
    }
}