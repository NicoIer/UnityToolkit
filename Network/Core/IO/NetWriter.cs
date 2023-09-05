using System;
using Nico;

namespace Nico
{
    public class NetWriter : IDisposable
    {
        private static class Writer<T>
        {
            public static Action<NetWriter, T> write;
        }

        private static readonly Pool<NetWriter> Pool = new Pool<NetWriter>(() => new NetWriter(), 1000);

        public static NetWriter Get()
        {
            NetWriter writer = Pool.Get();
            writer.Reset();
            return writer;
        }

        public static void Return(NetWriter writer) => Pool.Return(writer);

        public static void Register<T>(Action<NetWriter, T> writer)
        {
            Writer<T>.write = writer;
        }

        public void Dispose() => Pool.Return(this);

        public const int DefaultCapacity = 1500; //MTU~=1500
        public const int GrowScale = 2;
        internal byte[] buffer = new byte[DefaultCapacity];

        public int Position { get; private set; }

        public int Capacity => buffer.Length;

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

        private byte[] ToArray()
        {
            byte[] data = new byte[Position];
            Array.ConstrainedCopy(buffer, 0, data, 0, Position);
            return data;
        }

        private ArraySegment<byte> ToArraySegment()
        {
            return new ArraySegment<byte>(buffer, 0, Position);
        }

        public static implicit operator ArraySegment<byte>(NetWriter writer) => writer.ToArraySegment();

        public static implicit operator byte[](NetWriter writer) => writer.ToArray();

        public unsafe void WriteBlittable<T>(T value) where T : unmanaged
        {
            int size = sizeof(T);
            EnsureCapacity(Position + size);
            fixed (byte* ptr = &buffer[Position])
            {
#if UNITY_ANDROID
                T* valueBuffer = stackalloc T[1]{value};
                UnsafeUtility.MemCpy(ptr, valueBuffer, size);
#else
                *(T*)ptr = value;
#endif
            }
            Position += size;
        }

        static NetWriter()
        {
            Writer<int>.write = (writer, value) => writer.WriteBlittable(value);
            Writer<uint>.write = (writer, value) => writer.WriteBlittable(value);
            Writer<short>.write = (writer, value) => writer.WriteBlittable(value);
            Writer<ushort>.write = (writer, value) => writer.WriteBlittable(value);
            Writer<long>.write = (writer, value) => writer.WriteBlittable(value);
            Writer<ulong>.write = (writer, value) => writer.WriteBlittable(value);
            Writer<float>.write = (writer, value) => writer.WriteBlittable(value);
            Writer<double>.write = (writer, value) => writer.WriteBlittable(value);
            Writer<bool>.write = (writer, value) => writer.WriteBlittable(value);
            Writer<char>.write = (writer, value) => writer.WriteBlittable(value);
            Writer<byte>.write = (writer, value) => writer.WriteBlittable(value);
        }

        public override string ToString() =>
            $"[{ToArraySegment().ToHexString()} @ {Position}/{Capacity}]";

        public void Write<T>(T value)
        {
            Action<NetWriter, T> writeDelegate = Writer<T>.write;
            if (writeDelegate == null)
            {
                throw new ArgumentException($"No writer found for {typeof(T)}. Please register a reader first.");
            }
            else
            {
                writeDelegate(this, value);
            }
        }
    }
}