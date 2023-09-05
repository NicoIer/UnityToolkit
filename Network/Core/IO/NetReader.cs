// from Mirror NetworkReaderPool.cs

using System;
using System.IO;
using System.Runtime.CompilerServices;
using Nico;
using Unity.Collections.LowLevel.Unsafe;

namespace Nico
{
    public class NetReader : IDisposable
    {
        private static class Reader<T>
        {
            public static Func<NetReader, T> read;
        }

        #region Static

        private static readonly Pool<NetReader> Pool = new Pool<NetReader>(() => new NetReader(new byte[] { }), 1000);


        public static NetReader Get(byte[] bytes)
        {
            NetReader reader = Pool.Get();
            reader.SetBuffer(bytes);
            return reader;
        }

        public static NetReader Get(ArraySegment<byte> segment)
        {
            NetReader reader = Pool.Get();
            reader.SetBuffer(segment);
            return reader;
        }

        public void Dispose() => Pool.Return(this);


        /// <summary>
        /// 注册读取器
        /// </summary>
        /// <param name="reader"></param>
        /// <typeparam name="T"></typeparam>
        public static void Register<T>(Func<NetReader, T> reader)
        {
            Reader<T>.read = reader;
        }

        #endregion

        public const int AllocationLimit = 1024 * 1024 * 16; // 16MB * sizeof(T)

        internal ArraySegment<byte> buffer;

        public int Position { get; private set; }

        public int Remaining => buffer.Count - Position;

        public int Capacity => buffer.Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(NetReader reader) => Pool.Return(reader);


        private NetReader(ArraySegment<byte> segment)
        {
            buffer = segment;
        }

        public void SetBuffer(ArraySegment<byte> segment)
        {
            buffer = segment;
            Position = 0;
        }

        internal unsafe T ReadBlittable<T>() where T : unmanaged
        {
#if UNITY_EDITOR
            if (!UnsafeUtility.IsBlittable(typeof(T)))
            {
                throw new ArgumentException($"{typeof(T)} is not blittable!");
            }
#endif

            int size = sizeof(T);
            if (Remaining < size)
            {
                throw new EndOfStreamException(
                    $"ReadBlittable<{typeof(T)}> not enough data in buffer to read {size} bytes: {ToString()}");
            }

            T value;
            fixed (byte* ptr = &buffer.Array[buffer.Offset + Position])
            {
#if UNITY_ANDROID
                T* valueBuffer = stackalloc T[1];
                UnsafeUtility.MemCpy(valueBuffer, ptr, size);
                value = valueBuffer[0];
#else
                value = *(T*)ptr;
#endif
            }

            Position += size;
            return value;
        }

        public byte ReadByte() => ReadBlittable<byte>();

        public override string ToString() =>
            $"[{buffer.ToHexString()} @ {Position}/{Capacity}]";

        static NetReader()
        {
            Reader<int>.read = reader => reader.ReadBlittable<int>();
            Reader<uint>.read = reader => reader.ReadBlittable<uint>();
            Reader<short>.read = reader => reader.ReadBlittable<short>();
            Reader<ushort>.read = reader => reader.ReadBlittable<ushort>();
            Reader<long>.read = reader => reader.ReadBlittable<long>();
            Reader<ulong>.read = reader => reader.ReadBlittable<ulong>();
            Reader<float>.read = reader => reader.ReadBlittable<float>();
            Reader<double>.read = reader => reader.ReadBlittable<double>();
            Reader<bool>.read = reader => reader.ReadBlittable<bool>();
            Reader<char>.read = reader => reader.ReadBlittable<char>();
            Reader<byte>.read = reader => reader.ReadBlittable<byte>();
        }

        public T Read<T>()
        {
            Func<NetReader, T> readerDelegate = Reader<T>.read;
            if (readerDelegate == null)
            {
                throw new ArgumentException($"No reader found for {typeof(T)}. Please register a reader first.");
            }

            return readerDelegate(this);
        }
    }
}