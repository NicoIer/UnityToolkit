using UnityEngine;
using UnityEngine.Pool;

namespace UnityToolkit
{
    public class EasyGameObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform hidden;
        [SerializeField] private int initSize = 10;
        [SerializeField] private int maxSize = 100;

        private ObjectPool<GameObject> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<GameObject>(
                () => Instantiate(prefab),
                (obj) => obj.SetActive(true),
                (obj) =>
                {
                    obj.SetActive(false);
                    obj.transform.SetParent(hidden);
                }, DestroyImmediate, true, initSize, maxSize
            );
        }

        public GameObject Get() => _pool.Get();
        public void Release(GameObject obj) => _pool.Release(obj);
    }
}