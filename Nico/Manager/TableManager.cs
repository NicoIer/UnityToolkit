using System;
using System.Collections.Generic;
using Nico.Data;
using Nico.Design;
using Nico.Util;
using UnityEngine;

namespace Nico.Manager
{
    /// <summary>
    /// 表管理器
    /// TODO 有待完成
    /// </summary>
    public sealed class TableManager : GlobalSingleton<TableManager>, IInitializable
    {
        public string dataPath = "Asset/Test/";
        private Dictionary<Type, IMetaDataContainer> _containers = new Dictionary<Type, IMetaDataContainer>();

        public void Init()
        {
            // 获取当前程序集中所有IMetaDataContainer的实现类
            
        }

        public T1 GetMetaData<T1>(int idx) where T1 : IMetaData
        {
            var type = typeof(T1);
            if (!_containers.ContainsKey(type))
            {
                Debug.LogError($"TableManager: 未找到类型{type.Name}的表格");
                return default;
            }

            return (T1)_containers[type].GetMetaData(idx);
        }
    }
}