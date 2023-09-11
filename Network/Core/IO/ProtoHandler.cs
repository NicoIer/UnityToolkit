using System;
using Google.Protobuf;

namespace Nico
{
    public static class ProtoHandler
    {
        /// <summary>
        /// 从缓冲区解包
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="data"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void UnPack<T>(ref T msg, ByteString data) where T : IMessage<T>, new()
        {
            msg.MergeFrom(data);
        }

        /// <summary>
        /// 把消息打包到缓冲区
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="msg"></param>
        /// <param name="type"></param>
        /// <typeparam name="T"></typeparam>
        public static void Pack<T>(ProtoBuffer buffer, T msg, uint type = 0) where T : IMessage<T>
        {
            using (ProtoBuffer body = ProtoBuffer.Get())
            {
                PacketHeader header = Get<PacketHeader>();

                header.Id = TypeId<T>.ID;
                if (type != 0)
                {
                    header.Type = type;
                }

                body.WriteProto(msg); //写入body
                header.Body = body.ToByteString();
                buffer.WriteProto(header); //写入头

                Return(header);
            }
        }

        public static T Get<T>() where T : IMessage<T>, new()
        {
            return ProtoPool<T>.Pool.Get();
        }

        public static void Return<T>(this T msg) where T : IMessage<T>, new()
        {
            ProtoPool<T>.Pool.Return(msg);
        }

        private static class ProtoPool<T> where T : IMessage<T>, new()
        {
            public static Pool<T> Pool = new Pool<T>(() => new T(), 10);
        }
    }
}