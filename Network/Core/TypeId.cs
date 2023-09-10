namespace Nico
{
    /// <summary>
    /// 比dict更快
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class TypeId<T>
    {
        public static readonly int ID = typeof(T).FullName.GetStableHash();
    }
}