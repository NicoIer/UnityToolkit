using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MemoryPack;

namespace UnityToolkit.MathTypes
{
    [MemoryPackable]
    [StructLayout(LayoutKind.Sequential)]
    public partial struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(Vector3 a)
        {
            return new Vector3(0f - a.x, 0f - a.y, 0f - a.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(Vector3 a, float d)
        {
            return new Vector3(a.x * d, a.y * d, a.z * d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(float d, Vector3 a)
        {
            return new Vector3(a.x * d, a.y * d, a.z * d);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(Vector3 a, float d)
        {
            return new Vector3(a.x / d, a.y / d, a.z / d);
        }

        public static readonly Vector3 zero = new Vector3(0, 0, 0);
        public static readonly Vector3 one = new Vector3(1, 1, 1);

        public override string ToString()
        {
            return $"Vector3({x}, {y}, {z})";
        }
        
        public static implicit operator Vector3(System.Numerics.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
        
        public static implicit operator System.Numerics.Vector3(Vector3 v)
        {
            return new System.Numerics.Vector3(v.x, v.y, v.z);
        }
        
        public static bool operator ==(Vector3 a, System.Numerics.Vector3 b)
        {
            return ToolkitMath.Approximately(a.x, b.X) && ToolkitMath.Approximately(a.y, b.Y) &&
                   ToolkitMath.Approximately(a.z, b.Z);
        }
        
        public static bool operator !=(Vector3 a, System.Numerics.Vector3 b)
        {
            // 任意一个不相等就返回true
            return !ToolkitMath.Approximately(a.x, b.X) || !ToolkitMath.Approximately(a.y, b.Y) ||
                   !ToolkitMath.Approximately(a.z, b.Z);
        }

#if UNITY_5_6_OR_NEWER
        // UnityEngine.Vector3 -> Network.Vector3
        public static implicit operator Vector3(UnityEngine.Vector3 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static implicit operator UnityEngine.Vector3(Vector3 v)
        {
            return new UnityEngine.Vector3(v.x, v.y, v.z);
        }

        public static bool operator ==(Vector3 a, UnityEngine.Vector3 b)
        {
            return ToolkitMath.Approximately(a.x, b.x) && ToolkitMath.Approximately(a.y, b.y) &&
                   ToolkitMath.Approximately(a.z, b.z);
        }

        public static bool operator !=(Vector3 a, UnityEngine.Vector3 b)
        {
            // 任意一个不相等就返回true
            return !ToolkitMath.Approximately(a.x, b.x) || !ToolkitMath.Approximately(a.y, b.y) ||
                   !ToolkitMath.Approximately(a.z, b.z);
        }
#endif

        public static bool operator ==(Vector3 a, Vector3 b)
        {
            return ToolkitMath.Approximately(a.x, b.x) && ToolkitMath.Approximately(a.y, b.y) &&
                   ToolkitMath.Approximately(a.z, b.z);
        }

        public static bool operator !=(Vector3 a, Vector3 b)
        {
            // 任意一个不相等就返回true
            return !ToolkitMath.Approximately(a.x, b.x) || !ToolkitMath.Approximately(a.y, b.y) ||
                   !ToolkitMath.Approximately(a.z, b.z);
        }

        public bool Equals(Vector3 other)
        {
            return this == other;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 LerpUnclamped(Vector3 a, Vector3 b, float t)
        {
            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }
    }
}