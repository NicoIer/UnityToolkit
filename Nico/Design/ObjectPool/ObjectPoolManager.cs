using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nico.Network.Singleton
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Singleton { get; private set; }
        [field: SerializeReference] private List<GameObject> prefabs = new();

        [field: SerializeReference] private readonly Dictionary<string, ObjectPool> _poolDict = new();

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
               
            Singleton = this;
            foreach (var prefab in prefabs)
            {
                var pool = new GameObject(prefab.name + "Pool").AddComponent<ObjectPool>();
                pool.prefab = prefab;
                pool.transform.SetParent(transform);
                _poolDict.TryAdd(prefab.name, pool);
            }
            
        }
        

        public GameObject GetObject(string name)
        {
            return _poolDict[name].Get();
        }

        public void ReturnObject(string name, GameObject go)
        {
            _poolDict[name].Return(go);
        }
    }
}