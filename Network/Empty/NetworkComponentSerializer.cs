// using System;
// using System.Collections.Generic;
// using Network;
//
// namespace Network
// {
//     public class NetworkComponentSerializer
//     {
//         private readonly Dictionary<ushort, Func<NetworkComponentPacket, INetworkComponent>> _handler =
//             new Dictionary<ushort, Func<NetworkComponentPacket, INetworkComponent>>();
//
//         public void Register<T>() where T : INetworkComponent, new()
//         {
//             ushort id = NetworkId<T>.Value;
//             Func<NetworkComponentPacket, INetworkComponent> handler = packet =>
//             {
//                 T component = new T();
//                 component.UpdateFromPacket(packet);
//                 return component;
//             };
//             if (!_handler.TryAdd(id, handler))
//             {
//                 NetworkLogger.Warning($"{nameof(INetworkComponent)} {typeof(T).Name} 已经注册到{this} 中");
//                 return;
//             }
//         }
//
//         public void Register(Type type)
//         {
//             ushort id = NetworkId.CalculateId(type);
//             Func<NetworkComponentPacket, INetworkComponent> handler = packet =>
//             {
//                 INetworkComponent component = (INetworkComponent)Activator.CreateInstance(type);
//                 component.UpdateFromPacket(packet);
//                 return component;
//             };
//             _handler.Add(id, handler);
//         }
//
//         public INetworkComponent Deserializer(ushort type, NetworkComponentPacket packet)
//         {
//             return _handler[type](packet);
//         }
//     }
// }