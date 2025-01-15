using System;
using MemoryPack;

namespace Network.Time
{
    [MemoryPackable]
    internal partial struct ClientSyncTimeMessage : INetworkMessage
    {
        public long sendMs;

        public static ClientSyncTimeMessage Now()
        {
            return new ClientSyncTimeMessage()
            {
                sendMs = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
        }
    }

    [MemoryPackable]
    internal partial struct ServerSyncTimeMessage : INetworkMessage
    {
        public long clientSendMs;
        public long serverReceiveMs;

        public static ServerSyncTimeMessage From(ref ClientSyncTimeMessage msg)
        {
            return new ServerSyncTimeMessage()
            {
                clientSendMs = msg.sendMs,
                serverReceiveMs = DateTimeOffset.Now.ToUnixTimeMilliseconds()
            };
        }
    }   
}