using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MemoryPack;
using MemoryPack.Compression;

namespace Network
{
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
        public readonly long sendTimeMs;

        public PingMessage(long sendTimeMs)
        {
            this.sendTimeMs = sendTimeMs;
        }
    }

    [MemoryPackable]
    public partial struct PongMessage : INetworkMessage
    {
        /// <summary>
        /// Ping方发送Ping的时间
        /// Pinger -> Ponger -> Pinger
        /// </summary>
        public readonly long sendTimeMs;

        public PongMessage(ref PingMessage pingMessage)
        {
            sendTimeMs = pingMessage.sendTimeMs;
        }
    }

    [MemoryPackable]
    public partial struct ServerTimestampMessage : INetworkMessage
    {
        /// <summary>
        /// 服务器发送的时间
        /// </summary>
        public readonly long serverSendMs;

        /// <summary>
        /// 服务器估计的RTT
        /// </summary>
        public readonly int serverAssumeRttMs;

        public ServerTimestampMessage(long serverSendMs, int serverAssumeRttMs)
        {
            this.serverSendMs = serverSendMs;
            this.serverAssumeRttMs = serverAssumeRttMs;
        }
    }
}