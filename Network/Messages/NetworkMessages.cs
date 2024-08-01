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

    [MemoryPackable]
    public partial struct NetworkPacket : INetworkMessage
    {
        public readonly ushort id;
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

    [MemoryPackable]
    public partial struct PingMessage : INetworkMessage
    {
        public readonly long sendTime;

        public PingMessage(long sendTime)
        {
            this.sendTime = sendTime;
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
        // 接收到Ping的时间
        public readonly long receivePingTime;

        // Ping方发送Ping的时间
        public readonly long sendPingTime;

        public PongMessage(ref PingMessage pingMessage)
        {
            sendPingTime = pingMessage.sendTime;
            receivePingTime = DateTime.UtcNow.Ticks;
        }

        public PongMessage(long sendPingTime)
        {
            this.sendPingTime = sendPingTime;
            receivePingTime = DateTime.UtcNow.Ticks;
        }
    }
}