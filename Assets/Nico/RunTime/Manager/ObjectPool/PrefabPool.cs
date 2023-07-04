using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Nico
{
    internal class PrefabPool
    {
        private readonly Queue<GameObject> _gameObjects = new Queue<GameObject>();
        private readonly GameObject _prefab;
        private readonly string _prefabName;

        internal PrefabPool(GameObject prefab, string prefabName, int defaultCount = 0)
        {
            this._prefab = prefab;
            this._prefabName = prefabName;
            for (int i = 0; i < defaultCount; i++)
            {
                var obj = UnityEngine.Object.Instantiate(_prefab);
                Return(obj);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal GameObject Get()
        {
            if (_gameObjects.Count == 0)
            {
                var obj1 = UnityEngine.Object.Instantiate(_prefab);
                obj1.name = _prefabName;
                obj1.SetActive(true);
                return obj1;
            }

            var obj2 = _gameObjects.Dequeue();
            obj2.name = _prefabName;
            obj2.SetActive(true);
            return obj2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Return(GameObject gameObject)
        {
            gameObject.name = _prefabName;
            gameObject.SetActive(false);
            _gameObjects.Enqueue(gameObject);
        }
    }
}