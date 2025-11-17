// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using MemoryPack;

namespace UnityToolkit.MathTypes
{
    [MemoryPackable]
    public partial struct Quaternion : IEquatable<Quaternion>
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static Quaternion identity => new Quaternion(0, 0, 0, 0);
        public override string ToString()
        {
            return $"Quaternion({x}, {y}, {z}, {w})";
        }
#if UNITY_5_6_OR_NEWER
        public static implicit operator Quaternion(UnityEngine.Quaternion q)
        {
            return new Quaternion(q.x, q.y, q.z, q.w);
        }

        public static implicit operator UnityEngine.Quaternion(Quaternion q)
        {
            return new UnityEngine.Quaternion(q.x, q.y, q.z, q.w);
        }

        public static bool operator ==(Quaternion a, UnityEngine.Quaternion b)
        {
            return ToolkitMath.Approximately(a.x, b.x) && ToolkitMath.Approximately(a.y, b.y) &&
                   ToolkitMath.Approximately(a.z, b.z) && ToolkitMath.Approximately(a.w, b.w);
        }

        public static bool operator !=(Quaternion a, UnityEngine.Quaternion b)
        {
            return !ToolkitMath.Approximately(a.x, b.x) || !ToolkitMath.Approximately(a.y, b.y) ||
                   !ToolkitMath.Approximately(a.z, b.z) || !ToolkitMath.Approximately(a.w, b.w);
        }
#endif

        public static bool operator ==(Quaternion a, Quaternion b)
        {
            return ToolkitMath.Approximately(a.x, b.x) && ToolkitMath.Approximately(a.y, b.y) &&
                   ToolkitMath.Approximately(a.z, b.z) && ToolkitMath.Approximately(a.w, b.w);
        }

        public static bool operator !=(Quaternion a, Quaternion b)
        {
            return !ToolkitMath.Approximately(a.x, b.x) || !ToolkitMath.Approximately(a.y, b.y) ||
                   !ToolkitMath.Approximately(a.z, b.z) || !ToolkitMath.Approximately(a.w, b.w);
        }


        public static Quaternion operator +(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
        }


        public static Quaternion operator -(Quaternion a, Quaternion b)
        {
            return new Quaternion(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
        }


        public static Quaternion operator *(Quaternion a, float b)
        {
            return new Quaternion(a.x * b, a.y * b, a.z * b, a.w * b);
        }

        public static Quaternion operator *(float a, Quaternion b)
        {
            return new Quaternion(b.x * a, b.y * a, b.z * a, b.w * a);
        }

        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion(
                a.w * b.x + a.x * b.w + a.y * b.z - a.z * b.y,
                a.w * b.y - a.x * b.z + a.y * b.w + a.z * b.x,
                a.w * b.z + a.x * b.y - a.y * b.x + a.z * b.w,
                a.w * b.w - a.x * b.x - a.y * b.y - a.z * b.z
            );
        }


        public bool Equals(Quaternion other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return obj is Quaternion other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = x.GetHashCode();
                hashCode = (hashCode * 397) ^ y.GetHashCode();
                hashCode = (hashCode * 397) ^ z.GetHashCode();
                hashCode = (hashCode * 397) ^ w.GetHashCode();
                return hashCode;
            }
        }

        public static Quaternion SlerpUnclamped(Quaternion a, Quaternion b, float f)
        {
            if (f <= 0.0f)
            {
                return a;
            }

            if (f >= 1.0f)
            {
                return b;
            }

            var cosHalfAngle = a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
            if (cosHalfAngle >= 1.0f || cosHalfAngle <= -1.0f)
            {
                return a;
            }

            if (cosHalfAngle < 0.0f)
            {
                b.x = -b.x;
                b.y = -b.y;
                b.z = -b.z;
                b.w = -b.w;
                cosHalfAngle = -cosHalfAngle;
            }

            float halfAngle = ToolkitMath.Acos(cosHalfAngle);
            float sinHalfAngle = ToolkitMath.Sin(halfAngle);
            if (sinHalfAngle == 0.0f)
            {
                return a;
            }

            float ratioA = ToolkitMath.Sin((1 - f) * halfAngle) / sinHalfAngle;
            float ratioB = ToolkitMath.Sin(f * halfAngle) / sinHalfAngle;
            return new Quaternion(a.x * ratioA + b.x * ratioB, a.y * ratioA + b.y * ratioB,
                a.z * ratioA + b.z * ratioB, a.w * ratioA + b.w * ratioB);
        }

        public static Quaternion Slerp(Quaternion fromRotation, Quaternion toRotation, float t)
        {
            t = ToolkitMath.Clamp01(t);
            return SlerpUnclamped(fromRotation, toRotation, t);
        }

        public static Quaternion Inverse(in Quaternion value)
        {
            float lengthSq = value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w;
            if (lengthSq != 0.0f)
            {
                float i = 1.0f / lengthSq;
                return new Quaternion(-value.x * i, -value.y * i, -value.z * i, value.w * i);
            }

            return value;
        }

        public static float Angle(Quaternion a, Quaternion b)
        {
            float dot = a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w;
            return ToolkitMath.Acos(ToolkitMath.Min(ToolkitMath.Abs(dot), 1.0f)) * 2.0f * ToolkitMath.Rad2Deg;
        }
    }
}