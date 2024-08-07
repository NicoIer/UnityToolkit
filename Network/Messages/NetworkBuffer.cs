using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Network
{
    public sealed class NetworkBuffer : IBufferWriter<byte>
    {
        public const int DefaultCapacity = 1500;
        internal byte[] buffer;
        public int Position { get; private set; }
        public int Capacity => buffer.Length;


        public NetworkBuffer()
        {
            buffer = new byte[DefaultCapacity];
        }

        public NetworkBuffer(int capacity)
        {
            buffer = new byte[capacity];
        }

        /// <summary>
        /// 写入数据
        /// </summary>
        /// <param name="count">数据长度</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            Position += count;
        }


        /// <summary>
        /// 获取缓冲区指定长度的内存
        /// </summary>
        /// <param name="sizeHint">长度</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            EnsureCapacity(Position + sizeHint);
            return buffer.AsMemory(Position, sizeHint);
        }

        /// <summary>
        /// 获取缓冲区指定长度的内存
        /// </summary>
        /// <param name="sizeHint">长度</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> GetSpan(int sizeHint = 0)
        {
            EnsureCapacity(Position + sizeHint);
            return buffer.AsSpan(Position, sizeHint);
        }

        /// <summary>
        /// 重置缓冲区
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            Position = 0;
        }

        /// <summary>
        /// 确保缓冲区容量
        /// </summary>
        /// <param name="capacity">目标容量</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int capacity)
        {
            if (buffer.Length >= capacity) return;
            int newCapacity = Math.Max(capacity, buffer.Length * 2);
            Array.Resize(ref buffer, newCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<byte>(NetworkBuffer buffer) =>
            buffer.buffer.AsMemory(0, buffer.Position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ArraySegment<byte>(NetworkBuffer buffer) =>
            new ArraySegment<byte>(buffer.buffer, 0, buffer.Position);
    }
}