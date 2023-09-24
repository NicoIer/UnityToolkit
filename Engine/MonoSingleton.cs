using System;
using UnityEngine;

namespace UnityToolkit
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _singleton;
        private static bool _isQuitting; //这里的静态变量不会被子类共享
        public bool dontDestroyOnLoad = false;

        [RuntimeInitializeOnLoadMethod]
        public static void ResetStatic()
        {
            _singleton = null;
            _isQuitting = false;
        }

        public static T Singleton
        {
            get
            {
                if (_singleton == null && !_isQuitting) //第一次访问 且 不是在退出时访问
                {
                    _singleton = FindObjectOfType<T>(); // 从场景中查找
                    if (_singleton == null) //找不到则new一个
                    {
                        _singleton = new GameObject($"[{typeof(T).Name}]").AddComponent<T>();
                    }

                    _singleton.OnInit(); //手动初始化
                }

                return _singleton;
            }
        }

        protected virtual void OnInit()
        {
            transform.SetParent(null);
            if (dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected virtual void Awake()
        {
            if (_singleton == null)
            {
                _singleton = this as T;
                _singleton.OnInit();
            }
            else if (_singleton != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
            if (_singleton == null) return;
            Destroy(_singleton.gameObject);
            _singleton = null;
        }
        

        protected virtual void OnDestroy()
        {
            if(_singleton != this) return;
            _singleton = null;
        }
    }
}