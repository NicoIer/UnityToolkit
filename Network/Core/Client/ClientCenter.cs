using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;

namespace Nico
{
    /// <summary>
    /// 网络中心
    /// 提供网络消息管理
    /// </summary>
    public class ClientCenter
    {
        private readonly Dictionary<int, Action<ByteString, int>> _handlers = new();

        public void OnData(PacketHeader header, int channel)
        {
            _handlers[header.Id](header.Body, channel);
        }

        public void Register<T>(Action<T, int> handler) where T : IMessage<T>
        {
            int id = TypeId<T>.id;
            _handlers[id] = (data, channel) =>
            {
                T msg = ProtoReader.Reader<T>.reader(data);
                handler(msg, channel);
            };
        }
    }
}