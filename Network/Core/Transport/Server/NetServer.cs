using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;

namespace Nico
{
    [Serializable]
    public class NetServer
    {
        private readonly ServerTransport _transport;
        public bool isRunning => _transport.Active();
        public event Action<int, TransportError, string> onError;
        public event Action<int> onDisconnected;
        public event Action<int> onConnected;
        public Action<int, ArraySegment<byte>, int> onDataReceived;
        public Action<int, ArraySegment<byte>, int> onDataSent;
        private readonly Dictionary<int, Action<int, ByteString, int>> _handlers;

        public NetServer(ServerTransport transport)
        {
            this._transport = transport;

            transport.onConnected += _OnConnected;
            transport.onDisconnected += _OnDisconnected;
            transport.onError += _OnError;
            transport.onDataReceived += _OnDataReceived;
            transport.onDataSent += _OnDataSent;
            
            _handlers = new Dictionary<int, Action<int, ByteString, int>>();
        }

        #region Transport Event

        private void _OnError(int connectId, TransportError error, string msg)
        {
            onError?.Invoke(connectId, error, msg);
        }

        private void _OnDisconnected(int connectId)
        {
            onDisconnected?.Invoke(connectId);
        }

        private void _OnConnected(int connectId)
        {
            onConnected?.Invoke(connectId);
        }

        private void _OnDataSent(int connectId, ArraySegment<byte> data, int channel)
        {
            onDataSent?.Invoke(connectId, data, channel);
        }

        private void _OnDataReceived(int connectId, ArraySegment<byte> data, int channel)
        {
            PacketHeader header = PacketHeader.Parser.ParseFrom(data);
            _handlers[header.Id](connectId, header.Body, channel);
            onDataReceived?.Invoke(connectId, data, channel);
        }

        public void Send<T>(int connectId, T msg, uint type = 0, int channelId = Channels.Reliable)
            where T : IMessage<T>, new()
        {
            using (ProtoBuffer buffer = ProtoBuffer.Get())
            {
                buffer.Pack(msg, type, channelId);
                _transport.Send(connectId, buffer.ToArraySegment(), channelId);
            }
        }

        public void SendToAll<T>(T msg, uint type = 0, int channelId = Channels.Reliable) where T : IMessage<T>, new()
        {
            using (ProtoBuffer buffer = ProtoBuffer.Get())
            {
                buffer.Pack(msg, type, channelId);
                _transport.SendToAll(buffer.ToArraySegment(), channelId);
            }
        }

        #endregion


        #region Life Loop

        public void Start()
        {
            _transport.Start();
        }

        public void Stop()
        {
            _transport.Stop();
            _transport.Shutdown();
        }

        public void OnEarlyUpdate()
        {
            _transport?.TickIncoming();
        }

        public void OnLateUpdate()
        {
            _transport?.TickOutgoing();
        }

        #endregion

        #region Handler

        public void Register<T>(Action<int, T, int> handler, bool replace = false) where T : IMessage<T>
        {
            int id = TypeId<T>.ID;
            if (_handlers.ContainsKey(id) && !replace)
            {
                throw new InvalidDataException($"handler for {typeof(T).Name} already exists");
            }

            _handlers[id] = (connectId, data, channel) =>
            {
                T msg = ProtoHandler.Reader<T>.reader(data);
                handler(connectId, msg, channel);
            };
        }

        #endregion
    }
}