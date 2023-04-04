using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Nico.Design
{
    public class ObjectPool : MonoBehaviour, IPool
    {
        private GameObject _prefab;
        private Type _objType;
        [ShowInInspector]private readonly LinkedList<GameObject> _pool = new LinkedList<GameObject>();

        public void SetPrefab(GameObject prefab, Type objType)
        {
            _prefab = prefab;
            _objType = objType;
        }


        public void Return<T>(T poolObj) where T : IPoolObj
        {
            poolObj.OnReturn();
            //将obj放回对象池
           
            var obj = poolObj.GetGameObject();
            obj.SetActive(false);
            obj.transform.SetParent(transform);
            _pool.AddLast(obj);
        }

        public T Get<T>() where T : IPoolObj
        {
            if (typeof(T) != _objType)
                throw new DesignException($"ObjectPool中的对象类型为{_objType} 与要获取的对象类型{typeof(T)}不一致");
            //如果对象池中有对象 则直接从对象池中获取
            if (_pool.Count > 0)
            {
                var node = _pool.First;
                _pool.RemoveFirst();
                var obj = node.Value.GetComponent<T>();
                obj.OnGet();
                return obj;
            }

            //如果对象池中没有对象 则创建一个新的对象
            IPoolObj o = _CreateObj();
            o.OnGet();
            return (T)o;
        }

        public IPoolObj Get(Type objType)
        {
            if (objType != _objType)
                throw new DesignException($"ObjectPool中的对象类型为{_objType} 与要获取的对象类型{objType}不一致");
            //如果对象池中有对象 则直接从对象池中获取
            if (_pool.Count > 0)
            {
                var node = _pool.First;
                _pool.RemoveFirst();
                var obj = node.Value.GetComponent(objType);
                IPoolObj poolObj = (IPoolObj)Convert.ChangeType(obj, objType);
                poolObj.OnGet();
                return poolObj;
            }

            IPoolObj o = _CreateObj();
            o.OnGet();
            return o;
        }

        private IPoolObj _CreateObj()
        {
            var newObj = Instantiate(_prefab, transform).GetComponent(_objType);
            IPoolObj poolObj = (IPoolObj)Convert.ChangeType(newObj, _objType);
            return poolObj;
        }
    }
}