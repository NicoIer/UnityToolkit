#if UNITY_5_6_OR_NEWER
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityToolkit
{
    /// <summary>
    /// 实现了这个接口的MonoBehaviour可以被对象池管理 类似于TagComponent
    /// </summary>
    public interface IPoolObject
    {
        public void OnGet();
        public void OnRelease();
    }

    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    public class GameObjectPoolManager : MonoSingleton<GameObjectPoolManager>, IOnlyPlayingModelSingleton
    {
        #region Default

        //默认的回收和获取回调
        private static readonly Action<GameObject> DefaultOnRecycle = obj =>
        {
            if (obj.TryGetComponent(out IPoolObject poolObj))
            {
                poolObj.OnRelease();
            }
            else
            {
                obj.SetActive(false);
            }
        };

        private static readonly Action<GameObject> DefaultOnGet = obj =>
        {
            if (obj.TryGetComponent(out IPoolObject poolObj))
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

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        private Dictionary<object, ObjectPool<GameObject>> _prefabDict;


        protected override void OnInit()
        {
            base.OnInit();

            _prefabDict = new Dictionary<object, ObjectPool<GameObject>>();
        }

        /// <summary>
        /// 注册一个对象池
        /// </summary>
        public static bool Create(object key, GameObject prefab, int initialPoolSize = 10, int maxPoolSize = 100,
            Action<GameObject> onGet = null,
            Action<GameObject> onRecycle = null,
            Action<GameObject> onDestroy = null)
        {
            if (Singleton == null)
            {
                Debug.LogError($"{nameof(GameObjectPoolManager)} is not initialized!");
                return false;
            }

            if (SingletonNullable._prefabDict.ContainsKey(key))
            {
                Debug.LogError($"{prefab.name},instanceId:{key} already exist!");
                return false;
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

            if (prefab.GetComponents<IPoolObject>().Length > 1)
            {
                Debug.LogError($"{prefab.name} has more than one {nameof(IPoolObject)} component! refuse to register");
                return false;
            }

            ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() =>
            {
                GameObject go = Instantiate(prefab);
                return go;
            }, onGet, onRecycle, onDestroy, true, initialPoolSize, maxPoolSize);

            SingletonNullable._prefabDict.Add(key, pool);
            return true;
        }

        public static bool Get<T>(object key, out T obj)
        {
            if (SingletonNullable == null)
            {
                Debug.LogError($"{nameof(GameObjectPoolManager)} is not initialized!");
                obj = default;
                return false;
            }

            ObjectPool<GameObject> pool = SingletonNullable._prefabDict[key];
            return pool.Get().TryGetComponent(out obj);
        }

        public static GameObject Get(object key)
        {
            if (SingletonNullable == null)
            {
                Debug.LogError($"{nameof(GameObjectPoolManager)} is not initialized!");
                return default;
            }

            return SingletonNullable._prefabDict[key].Get();
        }

        public static void Release(object key, GameObject go)
        {
            if (SingletonNullable == null)
            {
                Debug.LogError($"{nameof(GameObjectPoolManager)} is not initialized!");
                return;
            }
            
            if(!Singleton._prefabDict.ContainsKey(key))
            {
                GameObject.Destroy(go);
                Debug.LogWarning($"[{nameof(GameObjectPoolManager)}]:{key} not exist! Destroy the object!");
                return;
            }

            ObjectPool<GameObject> pool = Singleton._prefabDict[key];
            pool.Release(go);
        }

        public static void Destroy(object key)
        {
            if (SingletonNullable == null)
            {
                return;
            }

            SingletonNullable._prefabDict[key].Clear();
            SingletonNullable._prefabDict.Remove(key);
        }
    }
}
#endif