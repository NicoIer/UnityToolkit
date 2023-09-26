namespace UnityToolkit
{
    /// <summary>
    /// 比dict更快
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class TypeId<T>
    {
        public static readonly int StableId = typeof(T).FullName.GetStableHash();
        public static readonly int HashId = typeof(T).FullName.GetHashCode();
    }
}