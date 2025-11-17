// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using MemoryPack;
using UnityToolkit;
using UnityToolkit.MathTypes;

namespace Network
{
    [MemoryPackable]
    public partial struct TransformSnapshot
    {
        public long timestamp;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
    }

    public class NetworkTransform
    {
        public delegate long GetNetworkTimeDelegate();
        
        private GetNetworkTimeDelegate _getNetworkTime;
        private CircularBuffer<TransformSnapshot> _transformBuffer;
        private float interpBackTime;
        private float maxExtrapolationTime;
        private float snapThreshold;
        
        public NetworkTransform(
            GetNetworkTimeDelegate getNetworkTime,
            float interpBackTime = 0.1f,
            float maxExtrapolationTime = 0.25f,
            float snapThreshold = 2f,
            int bufferSize = 32
        )
        {
            _transformBuffer = new CircularBuffer<TransformSnapshot>(bufferSize);
            _getNetworkTime = getNetworkTime;
            this.interpBackTime = interpBackTime;
            this.maxExtrapolationTime = maxExtrapolationTime;
            this.snapThreshold = snapThreshold;
        }

        public void Tick(in float deltaTime)
        {
        }


        public void OnNetworkUpdate(TransformSnapshot snapshot)
        {
        }
    }
}