using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Network
{
    public class NetworkEntity
    {
        //服务器最大支持uint.MaxValue个网络身份 而且这个id不会复用
        //4294967295 假设一秒分配100个id 4294967295/100/60/60/24/365 = 1.36年 服务器该重启了
        public readonly uint id; // 唯一标识 由服务器分配
        public int owner; // 所有者 0表示服务器
        public List<NetworkComponent> components; // 组件列表


        private NetworkEntity()
        {
        }

        public NetworkEntity(uint id, int owner)
        {
            this.id = id;
            this.owner = owner;
            components = new List<NetworkComponent>();
        }

        /// <summary>
        /// 更新组件
        /// </summary>
        /// <param name="msg"></param>
        public void UpdateComponent(in NetworkComponentPacket msg)
        {
            Debug.Assert(msg.idx != null, "msg.idx != null");
            NetworkComponent component = components[msg.idx.Value];
            component.FromPacket(msg);
        }

        public static NetworkEntity From(uint id, int owner, in NetworkEntitySpawn req,
            NetworkComponentSerializer serializer)
        {
            NetworkEntity entity = new NetworkEntity(id, owner);
            Debug.Assert(entity.components == null);
            Debug.Assert(req.components.Count > 0);
            entity.components = new List<NetworkComponent>(req.components.Count);
            foreach (var packet in req.components)
            {
                Debug.Assert(packet.type != null, "packet.type != null");
                var component = serializer.Deserializer(packet.type.Value, packet);
                entity.components.Add(component);
            }

            return entity;
        }

        public void OnDestroy()
        {
            // TODO 回收组件
        }

        public void UpdateOwner(int newOwner)
        {
            this.owner = newOwner;
        }

        public void OnSpawn()
        {
            // TODO 初始化组件
        }
    }
}