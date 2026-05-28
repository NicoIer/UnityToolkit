// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Runtime.CompilerServices;

namespace UnityToolkit
{
    [Serializable]
    public class AnimationCurve
    {
        public enum WrapMode
        {
            Default = 0,
            Once = 1,
            Loop = 2,
            PingPong = 4,
            ClampForever = 8,
        }

        public enum WeightedMode
        {
            None = 0,
            In = 1,
            Out = 2,
            Both = 3,
        }

        [Serializable]
        public struct Keyframe
        {
            public float time;
            public float value;
            public float inTangent;
            public float outTangent;
            public float inWeight;
            public float outWeight;
            public WeightedMode weightedMode;

            public Keyframe(float time, float value)
            {
                this.time = time;
                this.value = value;
                inTangent = 0f;
                outTangent = 0f;
                inWeight = 1f / 3f;
                outWeight = 1f / 3f;
                weightedMode = WeightedMode.None;
            }

            public Keyframe(float time, float value, float inTangent, float outTangent)
            {
                this.time = time;
                this.value = value;
                this.inTangent = inTangent;
                this.outTangent = outTangent;
                inWeight = 1f / 3f;
                outWeight = 1f / 3f;
                weightedMode = WeightedMode.None;
            }

            public Keyframe(float time, float value, float inTangent, float outTangent, float inWeight,
                float outWeight)
            {
                this.time = time;
                this.value = value;
                this.inTangent = inTangent;
                this.outTangent = outTangent;
                this.inWeight = inWeight;
                this.outWeight = outWeight;
                weightedMode = WeightedMode.None;
            }
        }

#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField] private Keyframe[] _keys = Array.Empty<Keyframe>();
        [UnityEngine.SerializeField] private WrapMode _preWrapMode = WrapMode.ClampForever;
        [UnityEngine.SerializeField] private WrapMode _postWrapMode = WrapMode.ClampForever;
#else
        private Keyframe[] _keys = Array.Empty<Keyframe>();
        private WrapMode _preWrapMode = WrapMode.ClampForever;
        private WrapMode _postWrapMode = WrapMode.ClampForever;
#endif

        public Keyframe[] keys
        {
            get => _keys;
            set => _keys = value ?? Array.Empty<Keyframe>();
        }

        public WrapMode preWrapMode
        {
            get => _preWrapMode;
            set => _preWrapMode = value;
        }

        public WrapMode postWrapMode
        {
            get => _postWrapMode;
            set => _postWrapMode = value;
        }

        public int length => _keys.Length;

        public Keyframe this[int index] => _keys[index];

        public AnimationCurve()
        {
            _keys = Array.Empty<Keyframe>();
        }

        public AnimationCurve(params Keyframe[] keys)
        {
            _keys = keys ?? Array.Empty<Keyframe>();
        }

        public float Evaluate(float time)
        {
            if (_keys == null || _keys.Length == 0) return 0f;
            if (_keys.Length == 1) return _keys[0].value;

            float startTime = _keys[0].time;
            float endTime = _keys[_keys.Length - 1].time;
            float duration = endTime - startTime;

            if (duration <= 0f) return _keys[0].value;

            if (time < startTime)
                time = ApplyWrap(time, startTime, endTime, duration, _preWrapMode);
            else if (time > endTime)
                time = ApplyWrap(time, startTime, endTime, duration, _postWrapMode);

            // Binary search for the segment index (hi = right keyframe index)
            int hi = FindSegment(time);
            if (hi <= 0) return _keys[0].value;
            if (hi >= _keys.Length) return _keys[_keys.Length - 1].value;

            Keyframe k0 = _keys[hi - 1];
            Keyframe k1 = _keys[hi];

            float dt = k1.time - k0.time;
            if (dt <= 0f) return k0.value;

            // Stepped tangent: hold k0 value until k1
            if (float.IsInfinity(k0.outTangent)) return k0.value;

            bool useWeightedOut = (k0.weightedMode & WeightedMode.Out) != 0;
            bool useWeightedIn = (k1.weightedMode & WeightedMode.In) != 0;

            if (!useWeightedOut && !useWeightedIn)
            {
                float u = (time - k0.time) / dt;
                return EvalHermite(u, k0.value, k1.value, k0.outTangent * dt, k1.inTangent * dt);
            }

            float w0 = useWeightedOut ? k0.outWeight : 1f / 3f;
            float w1 = useWeightedIn ? k1.inWeight : 1f / 3f;
            return EvalWeightedBezier(time, k0.time, k1.time, k0.value, k1.value,
                k0.outTangent, k1.inTangent, w0, w1, dt);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float EvalHermite(float u, float v0, float v1, float m0, float m1)
        {
            float u2 = u * u;
            float u3 = u2 * u;
            return (2f * u3 - 3f * u2 + 1f) * v0
                   + (u3 - 2f * u2 + u) * m0
                   + (-2f * u3 + 3f * u2) * v1
                   + (u3 - u2) * m1;
        }

        private static float EvalWeightedBezier(float time, float t0, float t1,
            float v0, float v1, float m0, float m1, float w0, float w1, float dt)
        {
            // Bezier control point x positions
            float bx1 = t0 + w0 * dt;
            float bx2 = t1 - w1 * dt;

            // Newton-Raphson: find u in [0,1] s.t. B_x(u) == time
            float u = (time - t0) / dt;
            for (int i = 0; i < 16; i++)
            {
                float err = CubicBezier(u, t0, bx1, bx2, t1) - time;
                if (MathF.Abs(err) < 1e-6f) break;
                float deriv = CubicBezierDeriv(u, t0, bx1, bx2, t1);
                if (MathF.Abs(deriv) < 1e-12f) break;
                u -= err / deriv;
                if (u < 0f) u = 0f;
                else if (u > 1f) u = 1f;
            }

            // Bezier control point y positions
            float by1 = v0 + m0 * w0 * dt;
            float by2 = v1 - m1 * w1 * dt;
            return CubicBezier(u, v0, by1, by2, v1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CubicBezier(float t, float p0, float p1, float p2, float p3)
        {
            float mt = 1f - t;
            return mt * mt * mt * p0
                   + 3f * mt * mt * t * p1
                   + 3f * mt * t * t * p2
                   + t * t * t * p3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float CubicBezierDeriv(float t, float p0, float p1, float p2, float p3)
        {
            float mt = 1f - t;
            return 3f * mt * mt * (p1 - p0)
                   + 6f * mt * t * (p2 - p1)
                   + 3f * t * t * (p3 - p2);
        }

        // Returns the index of the first keyframe with time > target (right endpoint of segment)
        private int FindSegment(float time)
        {
            int lo = 0, hi = _keys.Length;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (_keys[mid].time <= time) lo = mid + 1;
                else hi = mid;
            }
            return lo;
        }

        private static float ApplyWrap(float time, float startTime, float endTime, float duration, WrapMode mode)
        {
            if (mode == WrapMode.Default) mode = WrapMode.ClampForever;
            switch (mode)
            {
                case WrapMode.Once:
                case WrapMode.ClampForever:
                    return time < startTime ? startTime : endTime;

                case WrapMode.Loop:
                {
                    float t = (time - startTime) % duration;
                    if (t < 0f) t += duration;
                    return startTime + t;
                }
                case WrapMode.PingPong:
                {
                    float cycle = 2f * duration;
                    float t = (time - startTime) % cycle;
                    if (t < 0f) t += cycle;
                    return t <= duration ? startTime + t : endTime - (t - duration);
                }
                default:
                    return time < startTime ? startTime : endTime;
            }
        }

#if UNITY_5_3_OR_NEWER
        public static implicit operator AnimationCurve(UnityEngine.AnimationCurve src)
        {
            if (src == null) return null;
            var srcKeys = src.keys;
            var dstKeys = new Keyframe[srcKeys.Length];
            for (int i = 0; i < srcKeys.Length; i++)
            {
                ref var sk = ref srcKeys[i];
                dstKeys[i] = new Keyframe
                {
                    time = sk.time,
                    value = sk.value,
                    inTangent = sk.inTangent,
                    outTangent = sk.outTangent,
                    inWeight = sk.inWeight,
                    outWeight = sk.outWeight,
                    weightedMode = (WeightedMode)(int)sk.weightedMode,
                };
            }
            return new AnimationCurve(dstKeys)
            {
                _preWrapMode = (WrapMode)(int)src.preWrapMode,
                _postWrapMode = (WrapMode)(int)src.postWrapMode,
            };
        }

        public static implicit operator UnityEngine.AnimationCurve(AnimationCurve src)
        {
            if (src == null) return null;
            var dstKeys = new UnityEngine.Keyframe[src.length];
            for (int i = 0; i < src.length; i++)
            {
                ref var sk = ref src._keys[i];
                dstKeys[i] = new UnityEngine.Keyframe(sk.time, sk.value, sk.inTangent, sk.outTangent,
                    sk.inWeight, sk.outWeight)
                {
                    weightedMode = (UnityEngine.WeightedMode)(int)sk.weightedMode,
                };
            }
            return new UnityEngine.AnimationCurve(dstKeys)
            {
                preWrapMode = (UnityEngine.WrapMode)(int)src._preWrapMode,
                postWrapMode = (UnityEngine.WrapMode)(int)src._postWrapMode,
            };
        }
#endif
    }
}
