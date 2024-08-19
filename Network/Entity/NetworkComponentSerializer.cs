using System;
using System.Collections.Generic;
using Network;

namespace Network
{
    public class NetworkComponentSerializer
    {
        private readonly Dictionary<ushort, Func<NetworkComponentPacket, NetworkComponent>> _handler =
            new Dictionary<ushort, Func<NetworkComponentPacket, NetworkComponent>>();

        public void Register<T>() where T : NetworkComponent, new()
        {
            ushort id = NetworkId<T>.Value;
            Func<NetworkComponentPacket, NetworkComponent> handler = packet =>
            {
                T component = new T();
                component.FromPacket(packet);
                return component;
            };
            if (!_handler.TryAdd(id, handler))
            {
                NetworkLogger.Warning($"{nameof(NetworkComponent)} {typeof(T).Name} 已经注册到{this} 中");
                return;
            }
        }

        public void Register(Type type)
        {
            ushort id = NetworkId.CalculateId(type);
            Func<NetworkComponentPacket, NetworkComponent> handler = packet =>
            {
                NetworkComponent component = (NetworkComponent)Activator.CreateInstance(type);
                component.FromPacket(packet);
                return component;
            };
            _handler.Add(id, handler);
        }

        public NetworkComponent Deserializer(ushort type, NetworkComponentPacket packet)
        {
            return _handler[type](packet);
        }
    }
}