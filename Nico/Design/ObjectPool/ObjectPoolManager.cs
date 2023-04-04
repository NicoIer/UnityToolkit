using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Nico.Design
{
    public class ObjectPoolManager : GlobalSingleton<ObjectPoolManager>
    {
        //对象池配置SO -> 或许可以用配置文件代替
        [field: SerializeField] public PoolSetting poolSetting;
        [field: SerializeReference] private List<GameObject> prefabs = new();
        [field: SerializeReference,ShowInInspector] Dictionary<Type, IPool> _poolDict = new Dictionary<Type, IPool>();

        protected override void Awake()
        {
            base.Awake();
            //如果配置文件不为空 则先加载配置文件中的预制体配置信息
            if (poolSetting != null)
            {
                prefabs.AddRange(poolSetting.prefabs);
            }

            //对每一个要生成的预制体进行初始化
            foreach (var prefab in prefabs)
            {
                //尝试从prefab中获取IPoolObj接口
                if (prefab.TryGetComponent(out IPoolObj poolObj))
                {
                    //如果获取成功
                    //获取对象的真实类型 用于创建对应的对象池
                    var objType = poolObj.GetType();
                    //如果对应的对象池还不存在 则创建一个对应对象的对象池
                    if (!_poolDict.ContainsKey(objType))
                    {
                        var pool = new GameObject($"{objType.Name}-Pool");
                        pool.transform.SetParent(transform);
                        // 创建对象池
                        var poolComponent = pool.AddComponent<ObjectPool>();
                        // 设置对象池的预制体
                        poolComponent.SetPrefab(prefab, objType);
                        // 添加对象池到字典中
                        _poolDict.Add(objType, poolComponent);
                    }
                }
            }
        }

        /// <summary>
        /// 此处的T必须是IPoolObj的实现类.已经使用class, IPoolObj, new()强制约束
        ///TODO 是否需要约束为MonoBehaviour?
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="DesignException"></exception>
        public T Get<T>() where T : class, IPoolObj, new()
        {
            //获取对象的真实类型
            var objType = typeof(T);
            //如果对象池中存在对应的对象池 则从对象池中获取对象
            if (_poolDict.ContainsKey(objType))
            {
                return _poolDict[objType].Get<T>();
            }

            throw new DesignException($"{objType} has no pool");
        }

        public IPoolObj Get(Type objType)
        {
            //如果对象池中存在对应的对象池 则从对象池中获取对象
            if (_poolDict.ContainsKey(objType))
            {
                return _poolDict[objType].Get(objType);
            }

            throw new DesignException($"{objType} has no pool");
        }

        public void Return(GameObject obj)
        {
            //尝试获取其IPoolObj接口
            if (obj.TryGetComponent(out IPoolObj poolObj))
            {
                Return(poolObj);
            }

            throw new DesignException($"{obj} is no an IPoolObj");
        }

        public void Return(IPoolObj obj)
        {
            //将对象放回对象池
            var objType = obj.GetType();
            if (_poolDict.ContainsKey(objType))
            {
                _poolDict[objType].Return(obj);
            }
            else
            {
                // 此时有两种考虑 第一种 创建一个新的对象池 第二种 抛出异常
                // 为了便于排错 这里选择抛出异常 
                throw new DesignException($"{objType} has no pool");
            }
        }
    }
}