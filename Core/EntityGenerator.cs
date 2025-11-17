// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Collections.Concurrent;
using System.Threading;

namespace UnityToolkit
{
    public interface IEntityIdGenerator<out TID>
    {
        /// <summary>
        /// 获取下一个唯一的实体ID。
        /// </summary>
        /// <returns>下一个唯一的实体ID。</returns>
        TID Next();
    }

    public class EntityIdGenerator : IEntityIdGenerator<uint>
    {
        private uint _currentId;
        private readonly object _lock = new object();

        // 可选：初始化起始ID（默认从1开始，0保留为无效ID）
        public EntityIdGenerator(uint startId = 1)
        {
            _currentId = startId;
        }

        public uint Next()
        {
            lock (_lock)
            {
                // 检查ID是否耗尽
                if (_currentId == uint.MaxValue)
                {
                    throw new System.OverflowException("Entity ID空间已耗尽！");
                }

                return _currentId++;
            }
        }
    }

    // 需要回收时可实现ID回收池：
    public class RecyclableIdGenerator : IEntityIdGenerator<uint>
    {
        private uint _currentId;
        private readonly ConcurrentQueue<uint> _recycledIds = new();
        private readonly object _lock = new object();

        public uint Next()
        {
            if (_recycledIds.TryDequeue(out uint id))
            {
                return id;
            }

            lock (_lock)
            {
                // 检查ID是否耗尽
                if (_currentId == uint.MaxValue)
                {
                    throw new System.OverflowException("Entity ID空间已耗尽！");
                }

                return _currentId++;
            }
        }

        public void Recycle(uint id) => _recycledIds.Enqueue(id);
    }
}