using System;
using System.Collections.Generic;
using MemoryPack;
using UnityToolkit;

namespace Network
{
    public sealed class NetworkClientMessageHandler : IDisposable
    {
        private Dictionary<ushort, Action<ArraySegment<byte>>> _handler;

        public NetworkClientMessageHandler()
        {
            _handler = new Dictionary<ushort, Action<ArraySegment<byte>>>(16);
        }

        public ICommand Add<T>(Action<T> handler) where T : INetworkMessage
        {
            ushort id = NetworkId<T>.Value;
            if (!_handler.ContainsKey(id))
            {
                _handler[id] = delegate { };
            }

            var warp = Warp(handler);
            _handler[id] += warp;
            return new CommonCommand(() =>
            {
                if (_handler != null && _handler.ContainsKey(id))
                    _handler[id] -= warp;
            });
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

        public void Dispose()
        {
            if (_handler != null)
            {
                _handler.Clear();
                _handler = null;
            }
            
        }
    }
}