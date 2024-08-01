using MemoryPack;

namespace Network.Client
{
    public static class Extensions
    {
        public static void Send<TMessage>(this IClientSocket socket, TMessage message) where TMessage : INetworkMessage
        {
            NetworkBuffer msgBuffer = NetworkBufferPool.Shared.Get();
            NetworkBuffer packetBuffer = NetworkBufferPool.Shared.Get();
            NetworkPacket packet = NetworkPacket.Pack(message, msgBuffer);

            MemoryPackSerializer.Serialize(packetBuffer, packet);
            socket.Send(packetBuffer);

            NetworkBufferPool.Shared.Return(msgBuffer);
            NetworkBufferPool.Shared.Return(packetBuffer);
        }
    }
}