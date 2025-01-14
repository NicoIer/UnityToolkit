// #if UNITY_EDITOR
// #pragma warning disable 0067
// #endif
//
// using System;
// using System.Collections.Generic;
// using UnityToolkit;
//
// #if UNITY_5_6_OR_NEWER
// using UnityEngine;
// using UnityEngine.Assertions;
// #else
// using System.Diagnostics;
// #endif
//
// namespace Network.Client
// {
//     public class EntityNetworkClient
//     {
//         public IClientSocket socket;
//         public NetworkClient client;
//         public NetworkEntityMgr selfEntityMgr { get; private set; }
//         public Dictionary<int, NetworkEntityMgr> otherEntityMgrs { get; private set; }
//
//         /// <summary>
//         /// 所有的网络实体 自己的和其他人的
//         /// </summary>
//         public Dictionary<uint, NetworkEntity> entities { get; private set; }
//
//         public int connectionId => client.connectionId;
//
//
//         /// <summary>
//         /// 针对不同类型的NetworkComponent的序列化器
//         /// </summary>
//         public NetworkComponentSerializer componentSerializer { get; private set; }
//
//         public NetworkBufferPool bufferPool { get; private set; }
//         public NetworkBufferPool<NetworkComponentPacket> componentBufferPool { get; private set; }
//
//         public event Action<NetworkEntity> OnEntitySpawned = delegate { };
//         public event Action OnEntityDestroyed;
//
//         public event Action OnEntityUpdated;
//         public event Action OnComponentUpdated;
//
//         public virtual void OnInit()
//         {
//             selfEntityMgr = new NetworkEntityMgr();
//             otherEntityMgrs = new Dictionary<int, NetworkEntityMgr>();
//             entities = new Dictionary<uint, NetworkEntity>();
//
//             componentSerializer = new NetworkComponentSerializer();
//             bufferPool = new NetworkBufferPool(16);
//             componentBufferPool = new NetworkBufferPool<NetworkComponentPacket>(16);
//
//             socket = new TelepathyClientSocket();
//             client = new NetworkClient(socket, 60);
//
//             // client.AddSystem<NetworkClientTime>();
//             client.AddMsgHandler<NetworkEntitySpawn>(OnEntitySpawn);
//             client.AddMsgHandler<NetworkEntityUpdate>(OnEntityUpdate);
//             client.AddMsgHandler<NetworkComponentUpdate>(OnComponentUpdate);
//             client.AddMsgHandler<NetworkEntityDestroy>(OnEntityDestroy);
//         }
//
//         public virtual void OnDispose()
//         {
//             client.Dispose();
//             selfEntityMgr.Clear();
//             foreach (var entityMgr in otherEntityMgrs.Values)
//             {
//                 entityMgr.Clear();
//             }
//
//             foreach (var entitiesValue in entities.Values)
//             {
//                 entitiesValue.OnDestroy();
//             }
//
//             entities.Clear();
//         }
//
//         private void OnEntitySpawn(NetworkEntitySpawn spawn)
//         {
//             Debug.Assert(connectionId != 0, "connectionId!=0");
//             NetworkEntity entity = NetworkEntity.From(spawn.id, spawn.owner, spawn, componentSerializer);
//             Debug.Assert(entity != null, "entity != null");
//             Debug.Assert(!entities.ContainsKey(entity.id), "!entities.ContainsKey(entity.id)");
//             entities.Add(entity.id, entity);
//             if (spawn.owner == connectionId)
//             {
//                 NetworkLogger.Info($"服务器为本地客户端生成了一个实体:{entity}");
//                 selfEntityMgr.Add(entity);
//             }
//             else
//             {
//                 if (!otherEntityMgrs.ContainsKey(spawn.owner))
//                 {
//                     otherEntityMgrs.Add(spawn.owner, new NetworkEntityMgr());
//                 }
//
//                 NetworkLogger.Info($"服务器为其他人[{spawn.owner}]生成了一个实体:{entity}");
//                 otherEntityMgrs[spawn.owner].Add(entity);
//             }
//
//             OnEntitySpawned(entity);
//         }
//
//         private void OnEntityDestroy(NetworkEntityDestroy obj)
//         {
//             throw new NotImplementedException();
//         }
//
//         private void OnComponentUpdate(NetworkComponentUpdate obj)
//         {
//             if (!obj.component.mask.HasFlag(NetworkComponentPacket.Mask.EntityId))
//             {
//                 NetworkLogger.Warning("NetworkComponentUpdate.entityId is null");
//                 return;
//             }
//
//             if (!obj.component.mask.HasFlag(NetworkComponentPacket.Mask.Idx))
//             {
//                 NetworkLogger.Warning("NetworkComponentUpdate.idx is null");
//                 return;
//             }
//
//             if (entities.ContainsKey(obj.component.entityId))
//             {
//                 entities[obj.component.entityId].UpdateComponent(obj.component);
//             }
//         }
//
//         private void OnEntityUpdate(NetworkEntityUpdate obj)
//         {
//             throw new NotImplementedException();
//         }
//
//         public void UpdateComponent(NetworkEntity entity, int idx)
//         {
//             Debug.Assert(entity.components.Count > idx, "entity.components.Count > idx");
//             Debug.Assert(entities.ContainsKey(entity.id), "entities.ContainsKey(entity.id)");
//             UpdateComponent(entity, idx, entity.components[idx]);
//         }
//
//         public void UpdateComponent(NetworkEntity entity, int idx, INetworkComponent local)
//         {
//             NetworkBuffer buffer = bufferPool.Get();
//             NetworkComponentPacket packet = local.ToDummyPacket(buffer);
//             packet.idx = idx;
//             packet.entityId = entity.id;
//             packet.mask = NetworkComponentPacket.Mask.EntityId | NetworkComponentPacket.Mask.Idx |
//                           NetworkComponentPacket.Mask.Type;
//             NetworkComponentUpdate updateMsg = new NetworkComponentUpdate(packet);
//             client.Send(updateMsg);
//             bufferPool.Return(buffer);
//         }
//
//         public void SpawnEntity(params INetworkComponent[] components)
//         {
//             NetworkBuffer<NetworkComponentPacket> componentBuffer = componentBufferPool.Get();
//             var list = ListPool<NetworkBuffer>.Get();
//             foreach (var component in components)
//             {
//                 NetworkBuffer buffer = bufferPool.Get();
//                 list.Add(buffer);
//                 NetworkComponentPacket packet = component.ToDummyPacket(buffer);
//                 componentBuffer.Write(packet);
//             }
//
//             NetworkEntitySpawn spawn = new NetworkEntitySpawn(0, client.connectionId, componentBuffer);
//             client.Send(spawn);
//             componentBufferPool.Return(componentBuffer);
//             foreach (var buffer in list)
//             {
//                 bufferPool.Return(buffer);
//             }
//
//             ListPool<NetworkBuffer>.Release(list);
//         }
//
//         public void SpawnEntity(IEnumerable<INetworkComponent> components)
//         {
//             NetworkBuffer<NetworkComponentPacket> componentBuffer = componentBufferPool.Get();
//             var list = ListPool<NetworkBuffer>.Get();
//             foreach (var component in components)
//             {
//                 NetworkBuffer buffer = bufferPool.Get();
//                 list.Add(buffer);
//                 NetworkComponentPacket packet = component.ToDummyPacket(buffer);
//                 componentBuffer.Write(packet);
//             }
//
//             NetworkEntitySpawn spawn = new NetworkEntitySpawn(0, client.connectionId, componentBuffer);
//             client.Send(spawn);
//             componentBufferPool.Return(componentBuffer);
//             foreach (var buffer in list)
//             {
//                 bufferPool.Return(buffer);
//             }
//
//             ListPool<NetworkBuffer>.Release(list);
//         }
//
//         public bool ContainsEntity(uint entityID)
//         {
//             return entities.ContainsKey(entityID);
//         }
//     }
// }