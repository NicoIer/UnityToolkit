using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MemoryPack;

namespace Network
{
    public interface INetworkMessage
    {
        public const int IdSize = sizeof(ushort);
    }

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
        public readonly ArraySegment<byte> payload;

        public NetworkPacket(ushort id, ArraySegment<byte> payload)
        {
            this.id = id;
            this.payload = payload;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NetworkPacket Pack<T>(in T message, NetworkBuffer buffer)
            where T : INetworkMessage
        {
            var id = NetworkId<T>.Value;
            MemoryPackSerializer.Serialize(buffer, message);
            return new NetworkPacket(id, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NetworkPacket Unpack(in ArraySegment<byte> data)
        {
            NetworkPacket packet = MemoryPackSerializer.Deserialize<NetworkPacket>(data);
            return packet;
        }
    }
    
    /// <summary>
    /// 服务器为客户端分配的连接ID
    /// </summary>
    [MemoryPackable]
    public partial struct AssignConnectionIdMessage : INetworkMessage
    {
        public readonly int id;

        public AssignConnectionIdMessage(int id)
        {
            this.id = id;
        }
    }

    
    /// <summary>
    /// Ping消息
    /// </summary>
    [MemoryPackable]
    public partial struct PingMessage : INetworkMessage
    {
        public readonly long sendTimeTicks;

        public PingMessage(long sendTimeTicks)
        {
            this.sendTimeTicks = sendTimeTicks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PingMessage Now()
        {
            return new PingMessage(DateTime.UtcNow.Ticks);
        }
    }

    [MemoryPackable]
    public partial struct PongMessage : INetworkMessage
    {
        /// <summary>
        /// Pong消息发送方接收到Ping的时间
        /// Pinger -> Ponger -> Pinger
        /// </summary>
        public readonly long receivePingTimeTicks;

        /// <summary>
        /// Ping方发送Ping的时间
        /// Pinger -> Ponger -> Pinger
        /// </summary>
        public readonly long sendPingTimeTicks;

        public PongMessage(ref PingMessage pingMessage)
        {
            sendPingTimeTicks = pingMessage.sendTimeTicks;
            receivePingTimeTicks = DateTime.UtcNow.Ticks;
        }
    }


    [MemoryPackable]
    public partial struct RttMessage : INetworkMessage
    {
        public readonly int rttMs;

        public RttMessage(int rttMs)
        {
            this.rttMs = rttMs;
        }
    }
}