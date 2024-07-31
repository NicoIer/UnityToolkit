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
            return type.FullName.GetStableHash();
        }
        
        /// <summary>
        /// 这个是消息通信的基础 需要版本不同编程语言 不同平台下 对同一个字符串的hash值都是一样的
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetStableHash(this string str)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in str)
                    hash = hash * 31 + c;
                return hash;
            }
        }
    }
}