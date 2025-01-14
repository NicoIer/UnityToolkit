using System;
using System.Runtime.CompilerServices;
using UnityToolkit;

namespace Network
{
    public sealed class NetworkBufferPool
    {
        [ThreadStatic] private static NetworkBufferPool _shared;
        

        public static NetworkBufferPool Shared
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_shared == null)
                {
                    _shared = new NetworkBufferPool();
                }

                return _shared;
            }
        }

        private readonly Pool<NetworkBuffer> _pool;

        public NetworkBufferPool(int initialCapacity = 0)
        {
            _pool = new Pool<NetworkBuffer>(
                () => new NetworkBuffer(),
                writer => writer.Clear(),
                initialCapacity
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NetworkBuffer Get()
        {
            return _pool.Get();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(NetworkBuffer writer)
        {
            _pool.Return(writer);
        }
    }


    public sealed class NetworkBufferPool<T>
    {
        [ThreadStatic] private static NetworkBufferPool<T> _shared;


        public static NetworkBufferPool<T> Shared
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (_shared == null)
                {
                    _shared = new NetworkBufferPool<T>();
                }

                return _shared;
            }
        }

        private readonly Pool<NetworkBuffer<T>> _pool;

        public NetworkBufferPool(int initialCapacity = 0)
        {
            _pool = new Pool<NetworkBuffer<T>>(
                () => new NetworkBuffer<T>(),
                writer => writer.Reset(),
                initialCapacity
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public NetworkBuffer<T> Get()
        {
            return _pool.Get();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(NetworkBuffer<T> writer)
        {
            _pool.Return(writer);
        }
    }
}