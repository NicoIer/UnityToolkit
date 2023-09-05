namespace Nico
{
    /// <summary>
    /// 比dict更快
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal static class TypeId<T>
    {
        public static int id = typeof(T).FullName.GetStableHash();
    }
}