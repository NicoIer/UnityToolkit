// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Runtime.CompilerServices;
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
        public delegate void OnNewTransformDelegate(in Vector3 position, in Quaternion rotation);

        public delegate long GetNetworkTimeDelegate();

        private GetNetworkTimeDelegate _getNetworkTime;
        private CircularBuffer<TransformSnapshot> _transformBuffer;
        private float interpBackTime;
        private float maxExtrapolationTime;
        private float snapThreshold;
        private float rotationSnapThreshold; // 新增旋转突变阈值(角度，例如30度)

        // EMA
        private ExponentialMovingAverageVector3 emaPosition;
        private ExponentialMovingAverageQuaternion emaRotation;

        public Vector3 smoothedPosition => emaPosition.Value;
        public Quaternion smoothedRotation => emaRotation.Value;
        private bool _initialized = false;

        public event OnNewTransformDelegate OnNewTransform = delegate { };

        /// <param name="emaN">EMA的N值，推荐5~16</param>
        /// <param name="rotationSnapThreshold">旋转阈值，单位: 度</param>
        public NetworkTransform(
            GetNetworkTimeDelegate getNetworkTime,
            float interpBackTime = 0.1f,
            float maxExtrapolationTime = 0.25f,
            float snapThreshold = 2f,
            int bufferSize = 32,
            int emaN = 8,
            float rotationSnapThreshold = 30f
        )
        {
            _transformBuffer = new CircularBuffer<TransformSnapshot>(bufferSize);
            _getNetworkTime = getNetworkTime;
            this.interpBackTime = interpBackTime;
            this.maxExtrapolationTime = maxExtrapolationTime;
            this.snapThreshold = snapThreshold;
            this.rotationSnapThreshold = rotationSnapThreshold;
            emaPosition = new ExponentialMovingAverageVector3(emaN);
            emaRotation = new ExponentialMovingAverageQuaternion(emaN);
        }

        public void Tick(in float deltaTime)
        {
            long nowTimestamp = _getNetworkTime();

            double now = nowTimestamp / 1000.0;
            double renderTimestamp = (now - interpBackTime) * 1000.0;

            if (_transformBuffer.Count == 0)
                return;

            if (!_initialized)
            {
                var first = _transformBuffer[0];
                emaPosition.Reset();
                emaRotation.Reset();
                emaPosition.Add(first.position);
                emaRotation.Add(first.rotation);
                _initialized = true;
            }

            // 保证插值区间
            while (_transformBuffer.Count >= 2 &&
                   _transformBuffer[1].timestamp <= renderTimestamp)
            {
                _transformBuffer.PopFront();
            }

            if (_transformBuffer.Count >= 2)
            {
                var from = _transformBuffer[0];
                var to = _transformBuffer[1];

                double t0 = from.timestamp;
                double t1 = to.timestamp;
                float t = t1 > t0 ? Math.Clamp((float)((renderTimestamp - t0) / (t1 - t0)), 0.0f, 1.0f) : 0.0f;

                Vector3 interpPos = Vector3.Lerp(from.position, to.position, t);
                Quaternion interpRot = Quaternion.Slerp(from.rotation, to.rotation, t);

                // 位置 snap
                if ((emaPosition.Value - interpPos).sqrMagnitude > snapThreshold * snapThreshold)
                {
                    emaPosition.Reset();
                    emaPosition.Add(interpPos);
                }
                else
                {
                    emaPosition.Add(interpPos);
                }

                // 旋转 snap （角度阈值）
                float angleDiff = Quaternion.Angle(emaRotation.Value, interpRot); // Unity: Angle 返回度
                if (angleDiff > rotationSnapThreshold)
                {
                    emaRotation.Reset();
                    emaRotation.Add(interpRot);
                }
                else
                {
                    emaRotation.Add(interpRot);
                }
            }
            else
            {
                // 只有一个快照，外推
                var last = _transformBuffer[0];
                float dt = Math.Min((float)((renderTimestamp - last.timestamp) * 0.001f), maxExtrapolationTime);
                Vector3 extrapolatedPos = last.position + last.velocity * dt;
                Quaternion extrapolatedRot = last.rotation;

                if ((emaPosition.Value - extrapolatedPos).sqrMagnitude > snapThreshold * snapThreshold)
                {
                    emaPosition.Reset();
                    emaPosition.Add(extrapolatedPos);
                }
                else
                {
                    emaPosition.Add(extrapolatedPos);
                }

                float angleDiff = Quaternion.Angle(emaRotation.Value, extrapolatedRot);
                if (angleDiff > rotationSnapThreshold)
                {
                    emaRotation.Reset();
                    emaRotation.Add(extrapolatedRot);
                }
                else
                {
                    emaRotation.Add(extrapolatedRot);
                }
            }

            OnNewTransform(smoothedPosition, smoothedRotation);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnNetworkUpdate(in TransformSnapshot snapshot)
        {
            _transformBuffer.PushBack(snapshot);
        }
    }
}