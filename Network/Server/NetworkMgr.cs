using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityToolkit;

namespace Network.Server
{
    /// <summary>
    /// 服务器端的网络服务器管理
    /// </summary>
    public class NetworkMgr : IServerSystem
    {
        protected NetworkServer server;
        protected readonly NetworkBufferPool<NetworkComponentPacket> componentBufferPool;
        protected readonly NetworkBufferPool byteBufferPool;

        /// <summary>
        /// 针对不同类型的NetworkComponent的序列化器
        /// </summary>
        public NetworkComponentSerializer componentSerializer { get; private set; }


        /// <summary>
        /// 所有的网络实体，客户端+服务器
        /// </summary>
        public Dictionary<uint, NetworkEntity> entities { get; private set; }

        /// <summary>
        /// 客户端拥有的网络实体
        /// </summary>
        public Dictionary<int, NetworkEntityMgr> ClientEntityMgrs { get; private set; }

        /// <summary>
        /// 服务器拥有的所有网络实体
        /// </summary>
        public NetworkEntityMgr serverEntityMgr { get; private set; }

        public const int ServerId = 0;
        private uint _currentEntityId;
        protected readonly List<ICommand> disposeCommands;

        public NetworkMgr()
        {
            byteBufferPool = new NetworkBufferPool(16);
            componentBufferPool = new NetworkBufferPool<NetworkComponentPacket>(16);
            serverEntityMgr = new NetworkEntityMgr();
            disposeCommands = new List<ICommand>();
            entities = new Dictionary<uint, NetworkEntity>();
            ClientEntityMgrs = new Dictionary<int, NetworkEntityMgr>();
            _currentEntityId = 0;
            componentSerializer = new NetworkComponentSerializer();
        }


        private uint IncreaseId()
        {
#if NET8_0
            Interlocked.Increment(ref _currentEntityId);
#else
            long cur = _currentEntityId; // uint 转long 不会丢失精度
            Interlocked.Increment(ref cur);// 如果是NetStandard2.0 则用long进行增加
            _currentEntityId = (uint)cur;// 然后再转回uint
#endif
            return _currentEntityId;
        }

        public virtual void OnInit(NetworkServer t)
        {
            this.server = t;
            this.server.socket.OnConnected += OnClientConnected;
            this.server.socket.OnDisconnected += OnClientDisconnected;
            disposeCommands.Add(this.server.AddMsgHandler<NetworkEntityDestroy>(OnNetworkEntityDestroy));
            disposeCommands.Add(this.server.AddMsgHandler<NetworkEntityOwnerUpdate>(OnNetworkEntityOwnerUpdate));
            disposeCommands.Add(this.server.AddMsgHandler<NetworkEntityUpdate>(OnNetworkEntityUpdate));
            disposeCommands.Add(this.server.AddMsgHandler<NetworkComponentUpdate>(OnNetworkComponentUpdate));
            disposeCommands.Add(this.server.AddMsgHandler<NetworkEntitySpawn>(OnNetworkEntitySpawn));
        }


        public virtual void Dispose()
        {
            server.socket.OnConnected -= OnClientConnected;
            server.socket.OnDisconnected -= OnClientDisconnected;

            foreach (var disposeCommand in disposeCommands)
            {
                disposeCommand.Execute();
            }

            disposeCommands.Clear();
        }

        private void OnClientDisconnected(int connectId)
        {
            // 删除 这个客户端拥有的所有网络实体
            foreach (var entity in ClientEntityMgrs[connectId].ownedEntities.Values)
            {
                entity.OnDestroy();
                entities.Remove(entity.id);
                NetworkLogger.Info($"销毁属于客户端{connectId}的网络实体{entity}");
                server.SendToAll<NetworkEntityDestroy>(new NetworkEntityDestroy(entity.id));
            }

            NetworkLogger.Info($"客户端{connectId}断开连接，销毁其NetworkEntityMgr");
            ClientEntityMgrs[connectId].ownedEntities.Clear();
            ClientEntityMgrs.Remove(connectId);
        }

        private void OnClientConnected(int connectId)
        {
            NetworkLogger.Info($"客户端{connectId}连接,为其创建一个NetworkEntityMgr");
            ClientEntityMgrs.Add(connectId, new NetworkEntityMgr());
            // 拿一个Buffer来存储所有组件
            var componentListBuffer = componentBufferPool.Get();
            var byteBuffer = byteBufferPool.Get();
            foreach (var (id, networkEntity) in entities)
            {
                componentListBuffer.Reset();
                // 预先分配空间
                componentListBuffer.Advance(networkEntity.components.Count);
                // 获取Span
                var span = componentListBuffer.GetSpan(networkEntity.components.Count);
                // 依次序列化
                for (var i = 0; i < networkEntity.components.Count; i++)
                {
                    byteBuffer.Reset();
                    var networkComponent = networkEntity.components[i];
                    NetworkComponentPacket packet = networkComponent.ToPacket(byteBuffer);
                    span[i] = packet;
                }

                var spawnMsg = new NetworkEntitySpawn(id, networkEntity.owner, componentListBuffer);
                NetworkLogger.Info($"发送网络实体{networkEntity}给客户端{connectId} 因为客户端刚刚连接");
                server.SendToAll<NetworkEntitySpawn>(spawnMsg);
            }

            byteBufferPool.Return(byteBuffer);
            componentBufferPool.Return(componentListBuffer);
        }

        #region Handle Message Req

        /// <summary>
        /// 生成一个新的网络实体
        /// </summary>
        /// <param name="entity"></param>
        private void NewEntitySpawn(NetworkEntity entity)
        {
            entities.Add(entity.id, entity);
            entity.OnSpawn();

            if (ClientEntityMgrs.ContainsKey(entity.owner))
            {
                ClientEntityMgrs[entity.owner].Add(entity);
            }

            if (entity.owner == ServerId)
            {
                serverEntityMgr.Add(entity);
            }
        }

        /// <summary>
        /// 客户端请求生成一个网络实体
        /// </summary>
        /// <param name="connectId"></param>
        /// <param name="req"></param>
        private void OnNetworkEntitySpawn(int connectId, NetworkEntitySpawn req)
        {
            if (req.id.HasValue)
            {
                NetworkLogger.Error($"客户端请求生成网络实体时id必须为null");
                return;
            }

            // 所有者必须是发送这个请求的客户端或者服务器 不能由客户端A请求生成一个属于客户端B的网络实体
            if (req.owner.HasValue && req.owner.Value != connectId && req.owner.Value != ServerId)
            {
                NetworkLogger.Error(
                    $"客户端{connectId}尝试生成一个属于客户端{req.owner.Value}的网络实体");
                return;
            }

            // 不给值表示客户端自己想要生成一个属于自己的网络实体
            int owner = req.owner.HasValue ? req.owner.Value : connectId;
            uint id = IncreaseId();
            NetworkEntity entity = NetworkEntity.From(id, owner, req, componentSerializer);
            Debug.Assert(entity.id == id && entity.owner == owner);
            NewEntitySpawn(entity);

            NetworkLogger.Info($"生成网络实体{entity.id}，所有者{entity.owner}");
            NetworkEntitySpawn spawn = new NetworkEntitySpawn(entity.id, entity.owner, req.components);
            server.SendToAll<NetworkEntitySpawn>(spawn);
        }


        /// <summary>
        /// 更新一个已经存在的网络实体的所有者
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="oldOwner"></param>
        /// <param name="newOwner"></param>
        private void ExistingEntityOwnerUpdate(NetworkEntity entity, int oldOwner, int newOwner)
        {
            entity.UpdateOwner(newOwner);
            if (ClientEntityMgrs.ContainsKey(oldOwner))
            {
                ClientEntityMgrs[oldOwner].Remove(entity);
            }

            if (ClientEntityMgrs.ContainsKey(newOwner))
            {
                ClientEntityMgrs[newOwner].Add(entity);
            }

            if (newOwner == ServerId)
            {
                serverEntityMgr.Add(entity);
            }
        }


        /// <summary>
        /// 客户端请求更改网络实体的所有者
        /// </summary>
        /// <param name="connectId"></param>
        /// <param name="req"></param>
        private void OnNetworkEntityOwnerUpdate(int connectId, NetworkEntityOwnerUpdate req)
        {
            if (!entities.ContainsKey(req.identityId))
            {
                NetworkLogger.Error($"实体{req.identityId}不存在");
                return;
            }

            NetworkEntity entity = entities[req.identityId];
            if (entity.owner != connectId)
            {
                NetworkLogger.Error(
                    $"Client {connectId} try to change owner of entity {req.identityId} but it's not the owner");
                NetworkLogger.Error(
                    $"客户端{connectId}尝试更改实体{req.identityId}的所有者，但它不是所有者");
                return;
            }

            if (!ClientEntityMgrs.ContainsKey(req.target))
            {
                NetworkLogger.Error(
                    $"客户端{connectId}尝试更改实体{req.identityId}的所有者，但目标客户端{req.target}不存在");
                return;
            }

            ExistingEntityOwnerUpdate(entity, connectId, req.target);


            // 广播给所有客户端
            NetworkEntityOwnerUpdate update = new NetworkEntityOwnerUpdate(req.identityId, req.target);
            server.SendToAll<NetworkEntityOwnerUpdate>(update);
        }

        private void DestroyNetworkEntity(NetworkEntity entity)
        {
            entity.OnDestroy();
            entities.Remove(entity.id);
            if (ClientEntityMgrs.ContainsKey(entity.owner))
            {
                ClientEntityMgrs[entity.owner].Remove(entity);
            }

            if (entity.owner == ServerId)
            {
                serverEntityMgr.Remove(entity);
            }
        }

        /// <summary>
        /// 客户端请求销毁一个网络实体
        /// </summary>
        /// <param name="connectId"></param>
        /// <param name="req"></param>
        private void OnNetworkEntityDestroy(int connectId, NetworkEntityDestroy req)
        {
            if (!entities.ContainsKey(req.id))
            {
                NetworkLogger.Error($"实体{req.id}不存在");
                return;
            }

            NetworkEntity entity = entities[req.id];
            if (entity.owner != connectId)
            {
                NetworkLogger.Error(
                    $"Client {connectId} try to destroy entity {req.id} but it's not the owner");
                NetworkLogger.Error(
                    $"客户端{connectId}尝试销毁实体{req.id}，但它不是所有者");
                return;
            }

            NetworkLogger.Info($"销毁网络实体{entity.id}，所有者{entity.owner}");
            DestroyNetworkEntity(entity);

            NetworkEntityDestroy destroy = new NetworkEntityDestroy(req.id);
            server.SendToAll<NetworkEntityDestroy>(destroy);
        }


        private void OnNetworkComponentUpdate(int connectId, NetworkComponentUpdate req)
        {
            if (!req.component.identityId.HasValue)
            {
                NetworkLogger.Error($"组件的identityId不能为空");
                return;
            }

            if (!entities.ContainsKey(req.component.identityId.Value))
            {
                NetworkLogger.Error($"实体{req.component.identityId.Value}不存在");
                return;
            }

            NetworkEntity entity = entities[req.component.identityId.Value];
            if (entity.owner != connectId)
            {
                NetworkLogger.Error($"客户端{connectId}尝试更新实体{entity}的组件，但它不是所有者");
                return;
            }

            if (req.component.idx == null)
            {
                NetworkLogger.Error($"组件的idx不能为空");
                return;
            }

            entity.components[req.component.idx.Value].FromPacket(req.component);

            server.SendToAll<NetworkComponentUpdate>(req);
        }

        private void OnNetworkEntityUpdate(int connectId, NetworkEntityUpdate req)
        {
            if (!entities.ContainsKey(req.identityId))
            {
                NetworkLogger.Error($"实体{req.identityId}不存在");
                return;
            }

            NetworkEntity entity = entities[req.identityId];
            if (entity.owner != connectId)
            {
                NetworkLogger.Error($"客户端{connectId}尝试更新实体{entity}，但它不是所有者");
                return;
            }

            foreach (var componentPacket in req.components)
            {
                if (componentPacket.idx == null)
                {
                    NetworkLogger.Error($"组件的idx不能为空");
                    return;
                }

                entity.components[componentPacket.idx.Value].FromPacket(componentPacket);
            }

            server.SendToAll<NetworkEntityUpdate>(req);
        }

        #endregion
    }
}