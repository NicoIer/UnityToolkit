// using System;
// using Network;
// using MemoryPack;
//
// namespace Network
// {
//     /// <summary>
//     /// 网络实体更新
//     /// </summary>
//     [MemoryPackable]
//     public partial struct NetworkEntityUpdate : INetworkMessage
//     {
//         public readonly uint identityId;
//         public readonly ArraySegment<NetworkComponentPacket> components;
//         
//         public NetworkEntityUpdate(uint identityId, ArraySegment<NetworkComponentPacket> components)
//         {
//             this.identityId = identityId;
//             this.components = components;
//         }
//     }
//
//     /// <summary>
//     /// 网络组件更新
//     /// 客户端发起 服务器转发
//     /// </summary>
//     [MemoryPackable]
//     public partial struct NetworkComponentUpdate : INetworkMessage
//     {
//         public readonly NetworkComponentPacket component;
//         
//         public NetworkComponentUpdate(NetworkComponentPacket component)
//         {
//             this.component = component;
//         }
//     }
//     
//
//
//     /// <summary>
//     /// 创建网络实体
//     /// 客户端发起 服务器转发
//     /// 服务也可以自己创建
//     /// </summary>
//     [MemoryPackable]
//     public partial struct NetworkEntitySpawn : INetworkMessage
//     {
//         public readonly uint id;
//         public readonly int owner;
//         public readonly ArraySegment<NetworkComponentPacket> components;
//
//         public NetworkEntitySpawn(uint id, int owner, ArraySegment<NetworkComponentPacket> components)
//         {
//             this.id = id;
//             this.owner = owner;
//             this.components = components;
//         }
//     }
//
//     /// <summary>
//     /// 销毁网络实体
//     /// </summary>
//     [MemoryPackable]
//     public partial struct NetworkEntityDestroy : INetworkMessage
//     {
//         public readonly uint id;
//         
//         public NetworkEntityDestroy(uint id)
//         {
//             this.id = id;
//         }
//     }
//
//     /// <summary>
//     /// 变更网络实体所有者
//     /// </summary>
//     [MemoryPackable]
//     public partial struct NetworkEntityOwnerUpdate : INetworkMessage
//     {
//         public readonly uint identityId;
//         public readonly int target;
//         
//         public NetworkEntityOwnerUpdate(uint identityId, int target)
//         {
//             this.identityId = identityId;
//             this.target = target;
//         }
//     }
// }