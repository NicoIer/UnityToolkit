using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using MemoryPack;
using MemoryPack.Compression;

namespace Network
{
    internal static class NetworkPacker
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Pack<T>(in T message, NetworkBuffer payloadBuffer, IBufferWriter<byte> packetBuffer)
            where T : INetworkMessage
        {
            var id = NetworkId<T>.Value;
            MemoryPackSerializer.Serialize(payloadBuffer, message);
            NetworkPacket packet = new NetworkPacket(id, payloadBuffer);
            MemoryPackSerializer.Serialize(packetBuffer, packet);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void PackCompressed<T>(in T message, NetworkBuffer payloadBuffer,
            IBufferWriter<byte> packetBuffer)
            where T : INetworkMessage
        {
            using (var compressor = new BrotliCompressor())
            {
                var id = NetworkId<T>.Value;
                MemoryPackSerializer.Serialize(payloadBuffer, message);
                NetworkPacket packet = new NetworkPacket(id, payloadBuffer);
                MemoryPackSerializer.Serialize(compressor, packet);
                compressor.CopyTo(packetBuffer);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool UnpackCompressed(in ArraySegment<byte> data, out NetworkPacket packet)
        {
            try
            {
                using (var decompressor = new BrotliDecompressor())
                {
                    var decompress = decompressor.Decompress(data);
                    packet = MemoryPackSerializer.Deserialize<NetworkPacket>(decompress);
                }

                return true;
            }
            catch (Exception e)
            {
                NetworkLogger.Error($"UnpackCompressed error: {e}");
                packet = default;
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool Unpack(in ArraySegment<byte> data, out NetworkPacket packet)
        {
            try
            {
                packet = MemoryPackSerializer.Deserialize<NetworkPacket>(data);
                return true;
            }
            catch (Exception e)
            {
                NetworkLogger.Error($"Unpack error: {e}");
                packet = default;
                return false;
            }
        }
    }
}