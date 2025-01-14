// using System;
// using MemoryPack;
// using UnityToolkit;
//
// namespace Network
// {
//     public interface INetworkComponent
//     {
//         void UpdateFromPacket(in NetworkComponentPacket packet);
//
//         NetworkComponentPacket ToDummyPacket(NetworkBuffer buffer);
//     }
//
//
//     /// <summary>
//     /// NetworkComponent对应的网络包
//     /// 通过type找到原始类型进行反序列化
//     /// </summary>
//     [MemoryPackable]
//     public partial struct NetworkComponentPacket : INetworkMessage
//     {
//         [Flags]
//         public enum Mask : byte
//         {
//             EntityId = 1 << 0,
//             Idx = 1 << 1,
//             Type = 1 << 2,
//         }
//
//         public Mask mask;
//         public uint entityId;
//         public int idx;
//         public ushort type;
//         public ArraySegment<byte> data;
//     }
// }