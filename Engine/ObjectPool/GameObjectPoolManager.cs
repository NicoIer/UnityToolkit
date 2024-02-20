using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace UnityToolkit
{
    public interface IPoolGameObject
    {
        void OnGet();
        void OnRecycle();
    }

    [DisallowMultipleComponent]
    public class GameObjectPoolManager : MonoSingleton<GameObjectPoolManager>
    {
        #region Default

        //默认的回收和获取回调
        private static readonly Action<GameObject> _defaultOnRecycle = obj =>
        {
            obj.transform.SetParent(null);
            obj.SetActive(false);
        };

        private static readonly Action<GameObject> _defaultOnGet = obj => { obj.SetActive(true); };

        private static readonly Action<GameObject> _defaultOnDestroy = GameObject.Destroy;

        private static readonly Func<GameObject, GameObject> _defaultCreateFunc = GameObject.Instantiate;

        #endregion

        public int initialPoolSize = 10;
        public int maxPoolSize = 100;
        [SerializeField] private List<GameObject> prefabList = new();

        private Dictionary<int, ObjectPool<GameObject>> _prefabDict;
        private Dictionary<Type, int> _typeDict;

        protected override void OnInit()
        {
            _prefabDict = new Dictionary<int, ObjectPool<GameObject>>(prefabList.Count);
            _typeDict = new Dictionary<Type, int>(prefabList.Count);

            foreach (var prefab in prefabList)
            {
                Register(prefab, initialPoolSize, maxPoolSize);
            }
        }

        protected override void OnDispose()
        {
            foreach (var pool in _prefabDict.Values)
            {
                pool.Dispose();
            }

            _prefabDict.Clear();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            //去重
            HashSet<GameObject> set = new HashSet<GameObject>(prefabList);
            prefabList = set.ToList();
            //去空
            prefabList.RemoveNull();
        }
#endif

#if ODIN_INSPECTOR
        [SerializeField] internal string path = "Assets/AddressablesResources/Prefabs";
        [Sirenix.OdinInspector.Button]
        public void LoadAll()
        {
            prefabList = new List<GameObject>();
            // 列出文件夹下所有文件
            string[] files = System.IO.Directory.GetFiles(path, "*.asset", System.IO.SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                string assetPath = file.Substring(file.IndexOf("Assets", StringComparison.Ordinal));
                GameObject asset = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                if (asset == null)
                {
                    Debug.LogError($"[DataManager] LoadAll Error: {assetPath} is not exist.");
                    continue;
                }

                prefabList.Add(asset);
            }
        }
#endif

        /// <summary>
        /// 注册一个对象池
        /// </summary>
        public void Register(GameObject prefab, int initialPoolSize = 10, int maxPoolSize = 100,
            Action<GameObject> onGet = null,
            Action<GameObject> onRecycle = null,
            Action<GameObject> onDestroy = null)

        {
            if (prefab == null)
            {
                Debug.LogWarning("prefab is null!");
                return;
            }

            if (prefab.TryGetComponent(out IPoolGameObject poolGameObject))
            {
                _typeDict.Add(poolGameObject.GetType(), prefab.GetInstanceID());
                onGet = obj => { obj.GetComponent<IPoolGameObject>().OnGet(); };
                onRecycle = obj => { obj.GetComponent<IPoolGameObject>().OnRecycle(); };
            }

            int id = prefab.GetInstanceID();
            if (_prefabDict.ContainsKey(id))
            {
                Debug.LogWarning($"{prefab.name},instanceId:{id} already exist!");
                return;
            }

            if (onGet == null)
            {
                onGet = _defaultOnGet;
            }

            if (onRecycle == null)
            {
                onRecycle = _defaultOnRecycle;
            }

            if (onDestroy == null)
            {
                onDestroy = _defaultOnDestroy;
            }


            ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() => _defaultCreateFunc(prefab), onGet, onRecycle,
                onDestroy, true,
                initialPoolSize, maxPoolSize);

            _prefabDict.Add(id, pool);
        }

        public GameObject Get(GameObject prefab)
        {
            ObjectPool<GameObject> pool = _prefabDict[prefab.GetInstanceID()];
            return pool.Get();
        }

        public void Return(GameObject prefab, GameObject go)
        {
            ObjectPool<GameObject> pool = _prefabDict[prefab.GetInstanceID()];
            pool.Release(go);
        }

        public T Get<T>() where T : MonoBehaviour, IPoolGameObject
        {
            ObjectPool<GameObject> pool = _prefabDict[_typeDict[typeof(T)]];
            return pool.Get().GetComponent<T>();
        }

        public void Return<T>(GameObject go) where T : MonoBehaviour, IPoolGameObject
        {
            ObjectPool<GameObject> pool = _prefabDict[_typeDict[typeof(T)]];
            pool.Release(go);
        }
    }
}