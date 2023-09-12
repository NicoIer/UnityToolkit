using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;

namespace Nico
{
    public struct ServerMsg<T>
    {
        public T msg;
        public int channelId;
    }


    [Serializable]
    public class NetClient
    {
        public string address;
        private readonly ClientTransport _transport;
        public bool connected => _transport.connected;

        public event Action OnDisconnected;
        public event Action OnConnected;

        public event Action<TransportError, string> OnError;

        public event Action<ArraySegment<byte>, int> OnDataReceived;

        // public delegate void Handler(IMessage msg, int channel);

        // delegate void Reader(ByteString data, int channel);

        public event Action<ArraySegment<byte>, int> OnDataSent;
        private readonly Dictionary<int, Action<ByteString, int>> _handlers;

        private EventCenter _eventCenter;

        public NetClient(ClientTransport transport, string address)
        {
            this._transport = transport;
            this.address = address;
            transport.onError += _OnError;
            transport.onDisconnected += _OnDisconnected;
            transport.onConnected += _OnConnected;
            transport.onDataReceived += _OnDataReceived;
            transport.onDataSent += _OnDataSent;

            _handlers = new Dictionary<int, Action<ByteString, int>>();

            _eventCenter = new EventCenter();
        }


        #region Transport Event

        private void _OnDataSent(ArraySegment<byte> data, int channelId)
        {
            OnDataSent?.Invoke(data, channelId);
        }

        private void _OnDataReceived(ArraySegment<byte> data, int channelId)
        {
            PacketHeader header = ProtoHandler.Get<PacketHeader>();
            ProtoHandler.UnPack(ref header, data);
            if (!_handlers.ContainsKey(header.Id))
            {
                throw new InvalidDataException($"unregistered message id:{header.Id}");
            }

            _handlers[header.Id](header.Body, channelId);
            OnDataReceived?.Invoke(data, channelId);
            header.Return();
        }

        private void _OnConnected()
        {
            OnConnected?.Invoke();
        }

        private void _OnError(TransportError error, string msg)
        {
            OnError?.Invoke(error, msg);
        }

        private void _OnDisconnected()
        {
            OnDisconnected?.Invoke();
        }

        #endregion

        #region Life Loop

        public void Start()
        {
            _transport.Connect(address);
        }

        public void Stop()
        {
            _transport.Disconnect();
            _transport.Shutdown();
        }

        public void OnEarlyUpdate()
        {
            _transport.TickIncoming();
        }

        public void OnLateUpdate()
        {
            _transport.TickOutgoing();
        }

        public void Send<T>(T msg, uint type = 0, int channelId = Channels.Reliable) where T : IMessage<T>
        {
            // 拿两个buffer 一个用来写头 一个用来写body
            using (ProtoBuffer buffer = ProtoBuffer.Get())
            {
                ProtoHandler.Pack(buffer, msg, type);
                _transport.Send(buffer.ToArraySegment(), channelId);
            }
        }

        #endregion


        #region Handler

        public void Listen<T>(Action<ServerMsg<T>> handler) where T : IMessage<T>, new()
        {
            int id = TypeId<T>.ID;

            if (!_handlers.ContainsKey(id))
            {
                _handlers[id] = (data, channel) =>
                {
                    T msg = ProtoHandler.Get<T>();
                    ProtoHandler.UnPack(ref msg, data);
                    _eventCenter.Fire(new ServerMsg<T>
                    {
                        msg = msg,
                        channelId = channel
                    });
                    msg.Return();
                };
            }
            _eventCenter.Listen(handler);
        }

        public void UnListen<T>(Action<ServerMsg<T>> handler) where T : IMessage<T>, new()
        {
            int id = TypeId<T>.ID;
            if (!_handlers.ContainsKey(id))
            {
                return;
            }

            _eventCenter.UnListen(handler);
        }

        #endregion
    }
}