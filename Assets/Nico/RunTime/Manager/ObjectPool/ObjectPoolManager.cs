using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Nico
{
    public static class ObjectPoolManager
    {
        private static readonly Dictionary<string, PrefabPool> _pool = new Dictionary<string, PrefabPool>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>() where T : IPoolObject, new() => ObjectPool<T>.Get();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return<T>(T obj) where T : IPoolObject, new() => ObjectPool<T>.Return(obj);

        // ObjectPoolManager在Editor模式下可以使用预制体池 但是在进入运行时模式后 会自动清空Editor下注册的预制体池
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            _pool.Clear();
            Application.quitting -= OnApplicationQuit;
            Application.quitting += OnApplicationQuit;
        }

        private static void OnApplicationQuit()
        {
            _pool.Clear();
        }


        public static void Register(GameObject prefab, string prefabName = null)
        {
            if (prefab == null)
            {
                Debug.LogWarning(" prefab is null");
                return;
            }

            if (prefabName == null)
            {
                prefabName = prefab.name;
            }

            if (_pool.ContainsKey(prefabName))
            {
                Debug.LogWarning($" prefab name:{prefabName} is already in pool");
                return;
            }

            _pool.Add(prefabName, new PrefabPool(prefab, prefabName));
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject Get(string prefabName)
        {
            if (_pool.TryGetValue(prefabName, out var value))
            {
                return value.Get();
            }

            Debug.LogError(
                $"ObjectPoolManager.Get({prefabName}) failed. it has not been register into addressables yet. please using label{GlobalConst.POOL_OBJECT_PREFAB_LABEL} to tag it.");
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(GameObject gameObject)
        {
            if (_pool.TryGetValue(gameObject.name, out var value))
                value.Return(gameObject);
            else
            {
                Debug.LogWarning(
                    $"ObjectPoolManager.Return({gameObject.name}). it has not been register into addressables yet. please using label{GlobalConst.POOL_OBJECT_PREFAB_LABEL} to tag it. now will create a temp pool to store it.");
                var pool = new PrefabPool(gameObject, gameObject.name);
                pool.Return(gameObject);
                _pool.Add(gameObject.name, pool);
            }
        }
    }
}