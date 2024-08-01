using System;
using System.Collections.Generic;
using MemoryPack;

namespace Network
{
    public sealed class NetworkClientMessageHandler
    {
        private Dictionary<ushort, Action<ArraySegment<byte>>> _handler;

        public NetworkClientMessageHandler()
        {
            _handler = new Dictionary<ushort, Action<ArraySegment<byte>>>(16);
        }

        public void Add<T>(Action<T> handler) where T : INetworkMessage
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

        public void Handle(ushort id, in ArraySegment<byte> data)
        {
            if (_handler.ContainsKey(id))
            {
                _handler[id](data);
            }
        }

        public void Handle<T>(in ArraySegment<byte> data) where T : INetworkMessage
        {
            ushort id = NetworkId<T>.Value;
            if (_handler.ContainsKey(id))
            {
                _handler[id](data);
            }
        }


        private Action<ArraySegment<byte>> Warp<T>(Action<T> handler) where T : INetworkMessage
        {
            return data =>
            {
                T message = MemoryPackSerializer.Deserialize<T>(data);
                handler(message);
            };
        }
    }
}