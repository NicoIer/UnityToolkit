using MemoryPack;
using Network.Client;
using Network.Server;

namespace Network
{
    public static class SocketExtensions
    {
        public static void Send<TMessage>(this IClientSocket socket, TMessage message, NetworkBufferPool pool)
            where TMessage : INetworkMessage
        {
            NetworkBuffer msgBuffer = pool.Get();
            NetworkBuffer packetBuffer = pool.Get();
            NetworkPacket packet = NetworkPacket.Pack(message, msgBuffer);

            MemoryPackSerializer.Serialize(packetBuffer, packet);
            socket.Send(packetBuffer);

            pool.Return(msgBuffer);
            pool.Return(packetBuffer);
        }

        public static void Send<TMessage>(this IServerSocket socket, int connectionId, TMessage message,
            NetworkBufferPool pool)
            where TMessage : INetworkMessage
        {
            NetworkBuffer msgBuffer = pool.Get();
            NetworkBuffer packetBuffer = pool.Get();
            NetworkPacket packet = NetworkPacket.Pack(message, msgBuffer);

            MemoryPackSerializer.Serialize(packetBuffer, packet);
            socket.Send(connectionId, packetBuffer);

            pool.Return(msgBuffer);
            pool.Return(packetBuffer);
        }
    }
}