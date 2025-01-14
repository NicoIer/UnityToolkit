// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
//
// namespace Network
// {
//     public class NetworkEntity
//     {
//         //服务器最大支持uint.MaxValue个网络身份 而且这个id不会复用
//         //4294967295 假设一秒分配100个id 4294967295/100/60/60/24/365 = 1.36年 服务器该重启了
//         public readonly uint id; // 唯一标识 由服务器分配
//         public int owner; // 所有者 0表示服务器
//         public List<INetworkComponent> components; // 组件列表
//
//
//         public static event Action<NetworkEntity> OnEntitySpawned = delegate { };
//         public static event Action<NetworkEntity> OnEntityDespawned = delegate { };
//         public static event Action<NetworkEntity, INetworkComponent> OnComponentAdded = delegate { };
//         public static event Action<NetworkEntity, INetworkComponent> OnComponentUpdated = delegate { };
//         public static event Action<NetworkEntity, INetworkComponent> OnComponentRemoved = delegate { };
//
// #if UNITY_EDITOR
//         [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
//         public static void RestStatic()
//         {
//             OnComponentAdded = delegate { };
//             OnComponentUpdated = delegate { };
//             OnComponentRemoved = delegate { };
//             OnEntitySpawned = delegate { };
//             OnEntityDespawned = delegate { };
//         }
// #endif
//
//
//         private NetworkEntity()
//         {
//         }
//
//         public NetworkEntity(uint id, int owner)
//         {
//             this.id = id;
//             this.owner = owner;
//             components = new List<INetworkComponent>();
//         }
//
//         /// <summary>
//         /// 更新组件
//         /// </summary>
//         /// <param name="msg"></param>
//         public void UpdateComponent(in NetworkComponentPacket msg)
//         {
//             Debug.Assert(msg.mask.HasFlag(NetworkComponentPacket.Mask.Idx), "msg.idx != null");
//             INetworkComponent component = components[msg.idx];
//             component.UpdateFromPacket(msg);
//             OnComponentUpdated(this, component);
//         }
//
//         public static NetworkEntity From(uint id, int owner, in NetworkEntitySpawn req,
//             NetworkComponentSerializer serializer)
//         {
//             NetworkEntity entity = new NetworkEntity(id, owner);
//             entity.components = new List<INetworkComponent>(req.components.Count);
//
//             if (req.components != null)
//             {
//                 foreach (var packet in req.components)
//                 {
//                     Debug.Assert(packet.mask.HasFlag(NetworkComponentPacket.Mask.Type), "packet.type != null");
//                     var component = serializer.Deserializer(packet.type, packet);
//                     entity.components.Add(component);
//                 }
//             }
//             
//             OnEntitySpawned(entity);
//
//
//             return entity;
//         }
//
//         public void OnDestroy()
//         {
//             // TODO 回收组件
//         }
//
//         public void UpdateOwner(int newOwner)
//         {
//             this.owner = newOwner;
//         }
//
//         public void OnSpawn()
//         {
//             // TODO 初始化组件
//         }
//     }
// }