using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityToolkit
{
    public interface IConfig
    {
    }

    public class DataManager : MonoSingleton<DataManager>
    {
        protected override bool DontDestroyOnLoad() => true;

        protected override void OnInit()
        {
            InitCache();
        }

        protected override void OnDispose()
        {
            _tableType2Index.Clear();
            _dataType2TableType.Clear();
            _isCacheInit = false;
        }

        [field: SerializeField] private List<ScriptableObject> _dataTables;
        private List<IDataTable> _dataTableList;
        private Dictionary<Type, int> _tableType2Index;
        private Dictionary<Type, Type> _dataType2TableType;
        private bool _isCacheInit;


        [field: SerializeField] private List<ScriptableObject> _configList;
        private Dictionary<Type, ScriptableObject> _type2Configs;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TData Get<TData>(int id) where TData : ITableData
        {
            IDataTable table = GetTable<TData>();
            if (table == null)
            {
                Debug.LogError($"[DataManager] Get Error: {typeof(TData)} is not exist.");
                return default;
            }

            return (TData)table.Get(id);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TData Get<TData>(Enum @enum) where TData : ITableData
        {
            int id = Convert.ToInt32(@enum);
            return Get<TData>(id);
        }

        public IDataTable GetTable<TData>() where TData : ITableData
        {
            if (!_isCacheInit)
            {
                InitCache();
            }

            Type dataType = typeof(TData);
            if (!_dataType2TableType.ContainsKey(dataType))
            {
                Debug.LogError($"[DataManager] GetTable Error: {dataType} is not exist.");
                return null;
            }

            // 根据数据类型获取表格类型
            Type tableType = _dataType2TableType[dataType];
            if (!_tableType2Index.ContainsKey(tableType))
            {
                Debug.LogError($"[DataManager] GetTable Error: {dataType} is not exist.");
                return null;
            }

            int index = _tableType2Index[tableType];
            return _dataTableList[index];
        }

        public TConfig Get<TConfig>() where TConfig : class, IConfig
        {
            if (!_isCacheInit)
            {
                InitCache();
            }

            Type type = typeof(TConfig);
            if (!_type2Configs.ContainsKey(type))
            {
                Debug.LogError($"[DataManager] Get Error: {type} is not exist.");
                return default;
            }

            return _type2Configs[type] as TConfig;
        }


        private void InitCache()
        {
            // DATA TABLE
            _dataTableList = new List<IDataTable>();
            _tableType2Index = new Dictionary<Type, int>();
            _dataType2TableType = new Dictionary<Type, Type>();

            for (int i = 0; i < _dataTables.Count; i++)
            {
                IDataTable table = _dataTables[i] as IDataTable;
                if (table == null)
                {
                    Debug.LogError($"[DataManager] InitCache Error: {typeof(IDataTable)} is not exist.");
                    continue;
                }

                Type type = table.GetType();
                _dataTableList.Add(table);
                _tableType2Index.Add(type, i);

                Type dataType = table.GetDataType();
                _dataType2TableType.Add(dataType, type);
            }

            // CONFIG
            _type2Configs = new Dictionary<Type, ScriptableObject>();
            for (int i = 0; i < _configList.Count; i++)
            {
                ScriptableObject config = _configList[i];
                if (config == null)
                {
                    Debug.LogError($"[DataManager] InitCache Error: null scriptable object.");
                    continue;
                }

                if (config is not IConfig)
                {
                    Debug.LogError($"[DataManager] InitCache Error: {config} is not IConfig.");
                    continue;
                }

                Type type = config.GetType();
                _type2Configs.Add(type, config);
            }
        }

        private void OnValidate()
        {
            // DATA TABLE
            if (_dataTables == null)
            {
                _dataTables = new List<ScriptableObject>();
            }

            for (int i = _dataTables.Count - 1; i >= 0; i--)
            {
                ScriptableObject dataTable = _dataTables[i];
                if (dataTable == null)
                {
                    _dataTables.RemoveAt(i);
                    continue;
                }

                if (dataTable is not IDataTable)
                {
                    _dataTables.RemoveAt(i);
                    continue;
                }
            }

            // CONFIG
            if (_configList == null)
            {
                _configList = new List<ScriptableObject>();
            }

            for (int i = _configList.Count - 1; i >= 0; i--)
            {
                ScriptableObject config = _configList[i];
                if (config == null)
                {
                    _configList.RemoveAt(i);
                    continue;
                }

                if (config is not IConfig)
                {
                    _configList.RemoveAt(i);
                    continue;
                }
            }
        }
#if UNITY_EDITOR

        [SerializeField] private string datablePath = "Assets/AddressablesResources/DataTable";

        [SerializeField] private string configPath = "Assets/AddressablesResources/Config";

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#else
        [ContextMenu("LoadAll")]
#endif

        public void LoadAll()
        {
            _dataTables = new List<ScriptableObject>();
            // 列出文件夹下所有文件
            string[] files = Directory.GetFiles(datablePath, "*.asset", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                string assetPath = file.Substring(file.IndexOf("Assets", StringComparison.Ordinal));
                ScriptableObject asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                if (asset == null)
                {
                    Debug.LogError($"[DataManager] LoadAll Error: {assetPath} is not exist.");
                    continue;
                }

                _dataTables.Add(asset);
            }

            _configList = new List<ScriptableObject>();
            // 列出文件夹下所有文件
            files = Directory.GetFiles(configPath, "*.asset", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                string assetPath = file.Substring(file.IndexOf("Assets", StringComparison.Ordinal));
                ScriptableObject asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                if (asset == null)
                {
                    Debug.LogError($"[DataManager] LoadAll Error: {assetPath} is not exist.");
                    continue;
                }

                _configList.Add(asset);
            }
        }
#endif
    }
}