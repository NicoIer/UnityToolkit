using System.Collections.Generic;
using UnityEngine;

namespace Nico
{
    public class ObjectPool : MonoBehaviour
    {
        public GameObject prefab;
        private readonly Queue<GameObject> _pool = new();

        public GameObject Get()
        {
            GameObject obj=null;
            obj = _pool.Count == 0 ? Instantiate(prefab) : _pool.Dequeue();
            if (obj.TryGetComponent(out IPoolObject poolObject))
            {
                poolObject.Get();
            }
            obj.SetActive(true);
            return obj;
        }

        public void Return(GameObject go)
        {
            go.transform.SetParent(transform);
            go.SetActive(false);
            _pool.Enqueue(go);
        }
    }
}