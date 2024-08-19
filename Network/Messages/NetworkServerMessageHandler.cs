using System;
using System.Collections.Generic;
using MemoryPack;
using UnityToolkit;

namespace Network
{
    public sealed class NetworkServerMessageHandler : IDisposable
    {
        private readonly Dictionary<ushort, Action<int, ArraySegment<byte>>> _handler;

        public NetworkServerMessageHandler()
        {
            _handler = new Dictionary<ushort, Action<int, ArraySegment<byte>>>(16);
        }

        public ICommand Add<T>(Action<int, T> handler) where T : INetworkMessage
        {
            ushort id = NetworkId<T>.Value;
            if (!_handler.ContainsKey(id))
            {
                _handler[id] = delegate { };
            }

            var warp = Warp(handler);
            _handler[id] += warp;
            return new CommonCommand(() => { _handler[id] -= warp; });
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
            else
            {
                NetworkLogger.Warning($"No handler for message {id}");
            }
        }

        public void Handle<T>(int connectionId, in ArraySegment<byte> data) where T : INetworkMessage
        {
            ushort id = NetworkId<T>.Value;
            if (_handler.ContainsKey(id))
            {
                _handler[id](connectionId, data);
            }
            else
            {
                NetworkLogger.Warning($"No handler for message {typeof(T).Name}");
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

        public void Dispose()
        {
            _handler.Clear();
        }
    }
}