using UnityEngine;
using UnityEngine.Pool;

namespace UnityToolkit
{
    public class EasyGameObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private RectTransform _hidden;
        [SerializeField] private int initSize = 10;
        [SerializeField] private int maxSize = 100;

        private ObjectPool<GameObject> _pool;

        private void Awake()
        {
            _pool = new ObjectPool<GameObject>(
                () => Instantiate(_prefab),
                (obj) => obj.SetActive(true),
                (obj) =>
                {
                    obj.SetActive(false);
                    obj.transform.SetParent(_hidden);
                }, DestroyImmediate, true, initSize, maxSize
            );
            gameObject.SetActive(false);
        }

        public GameObject Get() => _pool.Get();
        public void Release(GameObject obj) => _pool.Release(obj);
    }
}