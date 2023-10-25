namespace UnityToolkit
{
    public interface IDataTable
    {
        public ITableData Get(int id);
#if UNITY_EDITOR
        public void Add(ITableData tableData);
#endif
    }
}