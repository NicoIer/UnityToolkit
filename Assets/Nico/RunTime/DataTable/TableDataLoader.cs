using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Nico
{
    public static class TableDataLoader
    {
        public static bool LoadFromResources<TDataTable>(string address,out TDataTable dataTable) where TDataTable : IDataTable
        {
            dataTable = default;
            Type type = typeof(TDataTable);
            
            ScriptableObject table = Resources.Load<ScriptableObject>(address);
            if (table == null)
            {
                Debug.LogError($"[TableDataManager] Load {type.Name} failed");
                return false;
            }

            if (table is not TDataTable dataTable1)
            {
                Debug.LogError($"[TableDataManager] {type.Name} is not IDataTable");
                return false;
            }

            dataTable = dataTable1;
            return true;
        }

        public static bool BuildInLoad<TDataTable>(out TDataTable dataTable) where TDataTable : IDataTable
        {
            return LoadFromAddressables<TDataTable>("DataTable/" + typeof(TDataTable).Name,out dataTable);
        }

        public static bool LoadFromAddressables<TDataTable>(string address,out TDataTable dataTable) where TDataTable : IDataTable
        {
            dataTable = default;
            Type type = typeof(TDataTable);

            ScriptableObject table = Addressables
                .LoadAssetAsync<ScriptableObject>(address).WaitForCompletion();
            if (table == null)
            {
                Debug.LogError($"[TableDataManager] Load {type.Name} failed");
                return false;
            }

            if (table is not TDataTable dataTable1)
            {
                Debug.LogError($"[TableDataManager] {type.Name} is not IDataTable");
                return false;
            }

            dataTable = dataTable1;

            return true;
        }
    }
}