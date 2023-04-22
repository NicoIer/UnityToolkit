using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Nico.Data;
using Nico.Design;
using Nico.Util;
using UnityEngine;
using UnityEngine.AddressableAssets;


namespace Nico.Manager
{
    public class ContainerManager : GlobalSingleton<ContainerManager>
    {
        [field: SerializeReference] private Dictionary<Type, Container> containers = new();

        protected override void Awake()
        {
            base.Awake();
            var types = ReflectUtil.GetTypesByParentClass<Container>(AppDomain.CurrentDomain);
            foreach (var type in types)
            {
                // Container container = Resources.Load<Container>(type.Name);
                string key = $"Nico-Data/{type.Name}"; //TODO 这里有重名的危险
                Container container =
                    Addressables.LoadAssetAsync<ScriptableObject>(key).WaitForCompletion() as Container;
                container.Init();
                if (container == null) continue;
                var dataType = container.GetDataType();
                containers.TryAdd(dataType, container);
            }
        }

        [CanBeNull]
        public T Get<T>(int id) where T : ContainerData
        {
            var type = typeof(T);
            if (!containers.ContainsKey(type))
            {
                Debug.LogError($"ContainerManager: 未找到类型{type.Name}的表格");
            }

            return containers[type].Get<T>(id);
        }
    }
}