using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

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


        public static void Register(GameObject prefab, string prefabName = null, OnSpawnDelegate onSpawn = null,
           OnRecycleDelegate onRecycle = null)
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

            _pool.Add(prefabName, new PrefabPool(prefab, prefabName, onSpawn, onRecycle));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static GameObject Get(string prefabName)
        {
            if (_pool.TryGetValue(prefabName, out var value))
            {
                return value.Get();
            }

            Debug.LogError(
                $"ObjectPoolManager.Get({prefabName}) failed. it has not been register into addressables yet. please register it first.");
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(MonoBehaviour behaviour, string name = null)
        {
            Return(behaviour.gameObject,name);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Return(GameObject gameObject,string name=null)
        {
            if (name == null)
            {
                name = gameObject.name;
            }

            if (_pool.TryGetValue(name, out var value))
            {
                value.Return(gameObject);
            }
            else
            {
                Debug.LogWarning(
                    $"ObjectPoolManager.Return({gameObject.name}). it has not been register yet. please register it first. now will create a temp pool to store it.");
                var pool = new PrefabPool(gameObject, gameObject.name);
                pool.Return(gameObject);
                _pool.Add(name, pool);
            }
        }
    }
}