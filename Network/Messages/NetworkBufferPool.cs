using System;
using System.Runtime.CompilerServices;

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
                writer => writer.Reset(),
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
}