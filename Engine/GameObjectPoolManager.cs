using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityToolkit
{
    /// <summary>
    /// 
    /// </summary>
    [DisallowMultipleComponent]
    internal class PoolObjConfig : MonoBehaviour
    {
        internal int initialPoolSize = 10;
        internal int maxPoolSize = 100;
    }

    public interface IGameObjectPoolObject
    {
        public void OnGet();
        public void OnReturn();
    }

    [DisallowMultipleComponent]
    public class GameObjectPoolManager : MonoSingleton<GameObjectPoolManager>
    {
        #region Default

        //默认的回收和获取回调
        private static readonly Action<GameObject> DefaultOnRecycle = obj =>
        {
            if (obj.TryGetComponent(out IGameObjectPoolObject poolObj))
            {
                poolObj.OnReturn();
            }
            else
            {
                obj.SetActive(false);
            }
        };

        private static readonly Action<GameObject> DefaultOnGet = obj =>
        {
            if (obj.TryGetComponent(out IGameObjectPoolObject poolObj))
            {
                poolObj.OnGet();
            }
            else
            {
                obj.SetActive(true);
            }
        };

        private static readonly Action<GameObject> DefaultOnDestroy = obj => { GameObject.Destroy(obj); };

        #endregion

        [SerializeField] private List<GameObject> prefabList = new List<GameObject>();

        private Dictionary<int, ObjectPool<GameObject>> prefabDict;

        private Dictionary<Type, int> type2Id;

        protected override void OnInit()
        {
            base.OnInit();
            type2Id = new Dictionary<Type, int>();
            prefabDict = new Dictionary<int, ObjectPool<GameObject>>();


            foreach (var prefab in prefabList)
            {
                int id = prefab.GetInstanceID();
                if (prefab.TryGetComponent(out IGameObjectPoolObject poolObject))
                {
                    type2Id[poolObject.GetType()] = id;
                }

                int initialPoolSize = 10; //默认值
                int maxPoolSize = 100; //默认值
                if (prefab.TryGetComponent(out PoolObjConfig poolConfig))
                {
                    initialPoolSize = poolConfig.initialPoolSize;
                    maxPoolSize = poolConfig.maxPoolSize;
                }

                Register(prefab, initialPoolSize, maxPoolSize);
            }
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            //去重
            HashSet<GameObject> set = new HashSet<GameObject>(prefabList);
            prefabList = set.ToList();
            //去空
            for (int i = prefabList.Count - 1; i >= 0; --i)
            {
                if (prefabList[i] == null)
                {
                    prefabList.RemoveAt(i);
                }
            }
        }
#endif

        /// <summary>
        /// 注册一个对象池
        /// </summary>
        private void Register(GameObject prefab, int initialPoolSize = 10, int maxPoolSize = 100,
            Action<GameObject> onGet = null,
            Action<GameObject> onRecycle = null,
            Action<GameObject> onDestroy = null)

        {
            int id = prefab.GetInstanceID();
            if (prefabDict.ContainsKey(id))
            {
                throw new Exception($"{prefab.name},instanceId:{id} already exist!");
            }

            if (onGet == null)
            {
                onGet = DefaultOnGet;
            }

            if (onRecycle == null)
            {
                onRecycle = DefaultOnRecycle;
            }

            if (onDestroy == null)
            {
                onDestroy = DefaultOnDestroy;
            }

            ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() =>
            {
                GameObject go = GameObject.Instantiate(prefab);
                return go;
            }, onGet, onRecycle, onDestroy, true, initialPoolSize, maxPoolSize);

            prefabDict.Add(id, pool);
        }

        public GameObject Get(GameObject prefab)
        {
            ObjectPool<GameObject> pool = prefabDict[prefab.GetInstanceID()];
            return pool.Get();
        }

        public T Get<T>() where T : MonoBehaviour, IGameObjectPoolObject
        {
            int id = type2Id[typeof(T)];
            ObjectPool<GameObject> pool = prefabDict[id];
            return pool.Get().GetComponent<T>();
        }

        public T Get<T>(Vector3 position) where T : MonoBehaviour, IGameObjectPoolObject
        {
            int id = type2Id[typeof(T)];
            ObjectPool<GameObject> pool = prefabDict[id];
            GameObject go = pool.Get();
            go.transform.position = position;
            return go.GetComponent<T>();
        }

        public void Return(GameObject prefab, GameObject go)
        {
            ObjectPool<GameObject> pool = prefabDict[prefab.GetInstanceID()];
            pool.Release(go);
        }

        public void Return<T>(T poolObject) where T : MonoBehaviour, IGameObjectPoolObject
        {
            int id = type2Id[poolObject.GetType()];
            ObjectPool<GameObject> pool = prefabDict[id];
            pool.Release(poolObject.gameObject);
        }
    }
}