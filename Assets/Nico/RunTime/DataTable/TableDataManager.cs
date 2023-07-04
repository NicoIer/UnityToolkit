using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Nico
{
    public static class TableDataManager
    {
        internal static readonly Dictionary<Type, IDataTable> dataTables = new Dictionary<Type, IDataTable>();

        // 运行时 会自动清空Editor下注册的表格 因此 无需担心内存泄漏
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            dataTables.Clear();
            Application.quitting -= OnApplicationQuit;
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            dataTables.Clear();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Register<TDataTable>(TDataTable dataTable) where TDataTable : IDataTable
        {
            Type type = typeof(TDataTable);
            if (dataTables.ContainsKey(type))
            {
                Debug.LogWarning($"dataTable:{typeof(TDataTable).Name} already loaded");
                return;
            }

            dataTables[type] = dataTable;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TData Get<TData>(int id) where TData : ITableData
        {
            var type = typeof(TData);
            if (!dataTables.TryGetValue(type, out IDataTable dataTable))
            {
                throw new ArgumentException($"[TableDataManager] {type.Name} not found");
            }

            return (TData)dataTable.Get(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TDataTable Get<TDataTable>() where TDataTable : IDataTable
        {
            var type = typeof(TDataTable);
            if (!dataTables.TryGetValue(type, out IDataTable dataTable))
            {
                throw new ArgumentException($"[TableDataManager] {type.Name} not found");
            }

            return (TDataTable)dataTable;
        }
    }
}