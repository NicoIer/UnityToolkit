using System;
using System.Collections.Generic;
using Nico.Design;
using Nico.Util;
using UnityEngine;

namespace Nico.Manager
{
    /// <summary>
    /// 表管理器
    /// </summary>
    public sealed class TableManager : GlobalSingleton<TableManager>, IInitializable
    {
        public string dataPath = "Asset/Test/";
        private Dictionary<Type, IMetaDataContainer> _containers = new Dictionary<Type, IMetaDataContainer>();

        public void Initialize()
        {
            // 获取当前程序集中所有IMetaDataContainer的实现类
            var types = ReflectUtil.GetTypesByInterface<IMetaDataContainer>(AppDomain.CurrentDomain);
#if UNITY_EDITOR
            //查询指定文件夹下的所有Container资产
            var assetPaths = AssetUtil.GetScriptableObject<IMetaDataContainer>(dataPath);
            foreach (var type in types)
            {
                Debug.Log(type.Name);
            }
            //查询指定文件夹下的所有表格  
#endif
        }

        public T1 GetMetaData<T1>(int idx) where T1 : IMetaData
        {
            throw new NotImplementedException();
        }
    }
}