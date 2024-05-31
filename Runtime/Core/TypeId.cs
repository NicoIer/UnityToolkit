using System;

namespace UnityToolkit
{
    /// <summary>
    /// 比dict更快
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class TypeId<T>
    {
        public static readonly int StableId = typeof(T).FullName.GetStableHash();
    }

    public static class TypeId
    {
        public static int StableId(Type type)
        {
            return type.FullName.GetStableHash();
        }
    }
}