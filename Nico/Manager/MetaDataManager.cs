using System;
using System.Collections.Generic;
using Nico.Design;
using Nico.Util;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Nico.Manager
{
    /// <summary>
    /// 表数据管理器
    /// TODO 有待完成 
    /// </summary>
    public  class MetaDataManager : GlobalSingleton<MetaDataManager>
    {

        [field: SerializeReference] private Dictionary<Type, IMetaDataContainer> _containers = new();

        protected override void Awake()
        {
            base.Awake();
            // 从当前程序集中获取所有的 IMetaDataContainer 的实现类
            // 获取当前程序集中所有 IMetaDataContainer 和 So 的实现类
            var types = ReflectUtil.GetTypesByInterface<IMetaDataContainer>(AppDomain.CurrentDomain);
            foreach (var type in types)
            {
                //TODO 这里的查找方式需要优化 做一个编辑器拓展来存储所有的表格的路径 比较好 且表格是只读的 因此 可以使用AssetBundle||Resources来加载
                var key = $"{type.Name}";//TODO 这里可能有重名的危险
                //使用 addressable 加载资源
                if (Addressables.LoadAssetAsync<ScriptableObject>(key).WaitForCompletion() is IMetaDataContainer container)
                {
                    //找到这个 IMetaDataContainer 对应的 IMetaData的类型
                    var metaDataType = container.GetMetaType();
                    _containers.Add(metaDataType, container);
                }
            }
        }
        public T1 GetMetaData<T1>(int idx) where T1 : IMetaData
        {
            var type = typeof(T1);
            return (T1)_containers[type].GetMetaData(idx) ;
        }
        
    }
}