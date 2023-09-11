using System;
using Google.Protobuf;

namespace Nico
{
    /// <summary>
    ///  给Protobuf序列化提供的缓冲区
    /// </summary>
    public class ProtoBuffer : IDisposable
    {
        private static readonly Pool<ProtoBuffer> Pool = new Pool<ProtoBuffer>(() => new ProtoBuffer(), 1000);
        public const int DefaultCapacity = 1500; //MTU~=1500
        public const int GrowScale = 2;

        internal byte[] buffer = new byte[DefaultCapacity];//1000*1500 byte ~= 1.5M
        public int Position { get; private set; }
        public int Capacity => buffer.Length;

        public static ProtoBuffer Get()
        {
            ProtoBuffer buffer = Pool.Get();
            buffer.Reset();
            return buffer;
        }

        public static void Return(ProtoBuffer buffer) => Pool.Return(buffer);

        public void Reset()
        {
            Position = 0;
        }

        private void EnsureCapacity(int value)
        {
            if (buffer.Length < value)
            {
                int capacity = Math.Max(value, buffer.Length * GrowScale);
                Array.Resize(ref buffer, capacity);
            }
        }

        /// <summary>
        /// 0-Position的数组段
        /// </summary>
        /// <returns></returns>
        public ArraySegment<byte> ToArraySegment()
        {
            return new ArraySegment<byte>(buffer, 0, Position);
        }

        public static implicit operator ArraySegment<byte>(ProtoBuffer writer) => writer.ToArraySegment();

        /// <summary>
        /// 把proto消息序列到缓冲区
        /// </summary>
        /// <param name="proto"></param>
        /// <typeparam name="T"></typeparam>
        public void WriteProto<T>(IMessage<T> proto) where T : IMessage<T>
        {
            int size = proto.CalculateSize();
            EnsureCapacity(Position + size);
            //使用数组段避免拷贝 进而避免GC
            //把消息写到缓冲区
            proto.WriteTo(new ArraySegment<byte>(buffer, Position, size));
            Position += size;
        }

        public ByteString ToByteString()
        {
            return ByteString.CopyFrom(new ArraySegment<byte>(buffer, 0, Position));
        }
        
        public override string ToString() =>
            $"[{ToArraySegment().ToHexString()} @ {Position}/{Capacity}]";

        // 禁止外部实例化
        private ProtoBuffer()
        {
        }

        public void Dispose() => Pool.Return(this);
    }
}