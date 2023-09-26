namespace UnityToolkit
{
    public interface ITableData
    {
#if UNITY_EDITOR
        public bool Parse(string[] values);
#endif
    }
}