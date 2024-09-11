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
    }
}