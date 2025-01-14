using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace Network
{
    /// <summary>
    /// 字节缓冲区
    /// </summary>
    public sealed class NetworkBuffer : IBufferWriter<byte>
    {
        public const int DefaultCapacity = 1500;
        internal byte[] buffer; // 1500byte = 1.5KB
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

        public NetworkBuffer(byte[] buffer)
        {
            this.buffer = buffer;
        }

        public NetworkBuffer(ArraySegment<byte> segment)
        {
            buffer = segment.Array;
            Position = segment.Offset + segment.Count;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            if (this.Position > buffer.Length - count)
            {
                throw new InvalidOperationException("Buffer overflow");
            }

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
            return buffer.AsMemory(Position);
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
            return buffer.AsSpan(Position);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            buffer.AsSpan(0, Position).Clear();
            Position = 0;
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ArraySegment<byte> ToArraySegment()
        {
            return new ArraySegment<byte>(buffer, 0, Position);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteBlittable<T>(T value)
            where T : unmanaged
        {
            // check if blittable for safety
// #if UNITY_EDITOR
//             if (!Unity.Collections.LowLevel.Unsafe.UnsafeUtility.IsBlittable(typeof(T)))
//             {
//                 UnityToolkit.ToolkitLog.Error($"{typeof(T)} is not blittable!");
//                 return;
//             }
// #endif
            unsafe
            {
                // calculate size
                //   sizeof(T) gets the managed size at compile time.
                //   Marshal.SizeOf<T> gets the unmanaged size at runtime (slow).
                // => our 1mio writes benchmark is 6x slower with Marshal.SizeOf<T>
                // => for blittable types, sizeof(T) is even recommended:
                // https://docs.microsoft.com/en-us/dotnet/standard/native-interop/best-practices
                int size = sizeof(T);

                // ensure capacity
                // NOTE that our runtime resizing comes at no extra cost because:
                // 1. 'has space' checks are necessary even for fixed sized writers.
                // 2. all writers will eventually be large enough to stop resizing.
                EnsureCapacity(Position + size);

                // write blittable
                fixed (byte* ptr = &buffer[Position])
                {
#if UNITY_ANDROID
                // on some android systems, assigning *(T*)ptr throws a NRE if
                // the ptr isn't aligned (i.e. if Position is 1,2,3,5, etc.).
                // here we have to use memcpy.
                //
                // => we can't get a pointer of a struct in C# without
                //    marshalling allocations
                // => instead, we stack allocate an array of type T and use that
                // => stackalloc avoids GC and is very fast. it only works for
                //    value types, but all blittable types are anyway.
                //
                // this way, we can still support blittable reads on android.
                // see also: https://github.com/vis2k/Mirror/issues/3044
                // (solution discovered by AIIO, FakeByte, mischa)
                T* valueBuffer = stackalloc T[1]{value};
                Unity.Collections.LowLevel.Unsafe.UnsafeUtility.MemCpy(ptr, valueBuffer, size);
#else
                    // cast buffer to T* pointer, then assign value to the area
                    *(T*)ptr = value;
#endif
                }

                Position += size;
            }
        }

        public static T ReadBlittable<T>(ref ArraySegment<byte> arraySegment)
            where T : unmanaged
        {
            if (arraySegment.Array == null)
            {
                throw new NullReferenceException("ArraySegment<byte>.Array is null");
            }

            //TODO  check if blittable for safety
            unsafe
            {
                int size = sizeof(T);

                fixed (byte* ptr = &arraySegment.Array[arraySegment.Offset])
                {
                    T value = *(T*)ptr;
                    arraySegment = new ArraySegment<byte>(arraySegment.Array, arraySegment.Offset + size,
                        arraySegment.Count - size);
                    return value;
                }
            }
        }
    }


    public sealed class NetworkBuffer<T> : IBufferWriter<T>
    {
        public const int DefaultCapacity = 1500;
        internal T[] buffer; // 1500 * sizeof(T)byte = 1.5KB * sizeof(T)
        public int Position { get; private set; }
        public int Capacity => buffer.Length;


        public NetworkBuffer()
        {
            buffer = new T[DefaultCapacity];
        }

        public NetworkBuffer(int capacity)
        {
            buffer = new T[capacity];
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Advance(int count)
        {
            if (this.Position > buffer.Length - count)
            {
                throw new InvalidOperationException("Buffer overflow");
            }
            Position += count;
        }


        /// <summary>
        /// 获取缓冲区指定长度的内存
        /// </summary>
        /// <param name="sizeHint">长度</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Memory<T> GetMemory(int sizeHint = 0)
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
        public Span<T> GetSpan(int sizeHint = 0)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(T value)
        {
            EnsureCapacity(Position + 1);
            buffer[Position++] = value;
        }

        /// <summary>
        /// 确保缓冲区容量
        /// </summary>
        /// <param name="capacity">目标容量</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void EnsureCapacity(int capacity)
        {
            if (buffer.Length >= capacity) return;
            int newCapacity = Math.Max(capacity, buffer.Length * 2);
            Array.Resize(ref buffer, newCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlyMemory<T>(NetworkBuffer<T> buffer) =>
            buffer.buffer.AsMemory(0, buffer.Position);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ArraySegment<T>(NetworkBuffer<T> buffer) =>
            new ArraySegment<T>(buffer.buffer, 0, buffer.Position);
    }
}