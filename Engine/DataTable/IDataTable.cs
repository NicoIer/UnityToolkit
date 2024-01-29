using System;

namespace UnityToolkit
{
    public interface IDataTable
    {
        public ITableData Get(int id);
        
        public Type GetDataType();
#if UNITY_EDITOR
        public void Add(ITableData tableData);
#endif
    }
}