using System;

namespace UnityToolkit.MathTypes
{
    [Serializable]
    public struct Vector2Int : IEquatable<Vector2Int>
#if UNITY_5_6_OR_NEWER
        , IEquatable<UnityEngine.Vector2Int>
#endif

    {
        public int x;
        public int y;

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public bool Equals(Vector2Int other)
        {
            return x == other.x && y == other.y;
        }

#if UNITY_5_6_OR_NEWER
        public bool Equals(UnityEngine.Vector2Int obj)
        {
            return x == obj.x && y == obj.y;
        }

        public static implicit operator UnityEngine.Vector2Int(Vector2Int v)
        {
            return new UnityEngine.Vector2Int(v.x, v.y);
        }

        public static implicit operator Vector2Int(UnityEngine.Vector2Int v)
        {
            return new Vector2Int(v.x, v.y);
        }

#endif
        
        public override int GetHashCode()
        {
            unchecked
            {
                return (x * 397) ^ y;
            }
        }
    }
}