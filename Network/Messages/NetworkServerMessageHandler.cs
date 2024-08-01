using System;
using System.Collections.Generic;
using MemoryPack;

namespace Network
{
    public sealed class NetworkServerMessageHandler
    {
        private Dictionary<ushort, Action<int, ArraySegment<byte>>> _handler;

        public NetworkServerMessageHandler()
        {
            _handler = new Dictionary<ushort, Action<int, ArraySegment<byte>>>(16);
        }

        public void Add<T>(Action<int, T> handler) where T : INetworkMessage
        {
            ushort id = NetworkId<T>.Value;
            if (!_handler.ContainsKey(id))
            {
                _handler[id] = delegate { };
            }

            _handler[id] += Warp(handler);
        }

        public void Clear<T>() where T : INetworkMessage
        {
            ushort id = NetworkId<T>.Value;
            if (_handler.ContainsKey(id))
            {
                _handler.Remove(id);
            }
        }

        public void Handle(ushort id, int connectionId, in ArraySegment<byte> data)
        {
            if (_handler.ContainsKey(id))
            {
                _handler[id](connectionId, data);
            }
        }

        public void Handle<T>(int connectionId, in ArraySegment<byte> data) where T : INetworkMessage
        {
            ushort id = NetworkId<T>.Value;
            if (_handler.ContainsKey(id))
            {
                _handler[id](connectionId, data);
            }
        }


        private Action<int, ArraySegment<byte>> Warp<T>(Action<int, T> handler) where T : INetworkMessage
        {
            return (connectionId, data) =>
            {
                T message = MemoryPackSerializer.Deserialize<T>(data);
                handler(connectionId, message);
            };
        }
    }
}