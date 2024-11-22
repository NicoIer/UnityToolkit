using System;

namespace UnityToolkit
{
    /// <summary>
    /// 比dict更快
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class TypeId<T>
    {
        public static readonly int stableId = TypeId.StableId(typeof(T));
    }

    public static class TypeId
    {
        public static int StableId(Type type)
        {
            return type.FullName.GetStableHashCode();
        }
        
        /// <summary>
        /// 这个是消息通信的基础 需要版本不同编程语言 不同平台下 对同一个字符串的hash值都是一样的
        /// </summary>
        /// <returns></returns>
        public static int GetStableHashCode(this string text)
        {
            unchecked
            {
                uint hash = 0x811c9dc5;
                uint prime = 0x1000193;

                for (int i = 0; i < text.Length; ++i)
                {
                    byte value = (byte)text[i];
                    hash = hash ^ value;
                    hash *= prime;
                }

                //UnityEngine.Debug.Log($"Created stable hash {(ushort)hash} for {text}");
                return (int)hash;
            }
        }

        // smaller version of our GetStableHashCode.
        // careful, this significantly increases chance of collisions.
        public static ushort GetStableHashCode16(this string text)
        {
            // deterministic hash
            int hash = GetStableHashCode(text);

            // Gets the 32bit fnv1a hash
            // To get it down to 16bit but still reduce hash collisions we cant just cast it to ushort
            // Instead we take the highest 16bits of the 32bit hash and fold them with xor into the lower 16bits
            // This will create a more uniform 16bit hash, the method is described in:
            // http://www.isthe.com/chongo/tech/comp/fnv/ in section "Changing the FNV hash size - xor-folding"
            return (ushort)((hash >> 16) ^ hash);
        }
    }
}