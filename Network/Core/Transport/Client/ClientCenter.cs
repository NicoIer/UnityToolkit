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

        /// <summary>
        /// 注册对指定消息的处理函数
        /// 这里只能注册一次，重复注册会覆盖
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="replace"></param>
        /// <typeparam name="T"></typeparam>
        public void Register<T>(Action<T, int> handler, bool replace = false) where T : IMessage<T>
        {
            int id = TypeId<T>.ID;

            if (_handlers.ContainsKey(id) && !replace)
            {
                throw new InvalidDataException($"handler for {typeof(T).Name} already exists");
            }

            _handlers[id] = (data, channel) =>
            {
                T msg = ProtoHandler.Reader<T>.reader(data);
                handler(msg, channel);
            };
        }
    }
}