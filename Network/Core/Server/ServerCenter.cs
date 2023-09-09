using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;


namespace Nico
{
    public class ServerCenter
    {
        private readonly Dictionary<int, Action<int, ByteString, int>> _handlers = new();

        public void OnData(int connectId, PacketHeader header, int channel)
        {
            _handlers[header.Id](connectId, header.Body, channel);
        }

        public void Register<T>(Action<int, T, int> handler) where T : IMessage<T>
        {
            int id = TypeId<T>.id;
            _handlers[id] = (connectId, data, channel) =>
            {
                T msg = ProtoReader.Reader<T>.reader(data);
                handler(connectId, msg, channel);
            };
        }
    }
}