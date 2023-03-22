using System;
using UnityEngine;

namespace Nico
{
    /// <summary>
    /// 跨场景的 Mono 单例模式 不会随着场景的切换而销毁
    /// 它是线程安全的
    /// </summary>
    public abstract class PersistentMonoSingleton<T> : MonoBehaviour, IMonoSingleton where T : PersistentMonoSingleton<T>
    {
        private static readonly object _lock = typeof(T);
        private static T _instance;
        private static bool _applicationQuit;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    //双重检查锁
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = FindObjectOfType<T>(); //从场景中寻找一个T类型的组件
                            if (_instance == null)
                            {
                                //找不见 就 new 一个
                                GameObject obj = new GameObject(typeof(T).Name);
                                _instance = obj.AddComponent<T>();
                            }
                        }
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            _applicationQuit = false;
            //如果Awake前没有被访问 那么就会在Awake中初始化
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if(_instance!=this)//这一步比较很重要
            {
                //如果已经被访问过了 代表已经有一个对应的单例对象存在了 那么就会在Awake中销毁自己
                Destroy(gameObject);
                return;
            }
        }

        private void OnApplicationQuit()
        {
            _applicationQuit = true;
        }

        public static T GetInstanceOnDisable(bool throwError = false)
        {
            if (_applicationQuit)
            {
                if (throwError)
                {
                    throw new Exception("Application is quitting !!!!");
                }

                return null;
            }

            return Instance;
        }
        protected virtual void OnDestroy()
        {
            //当单例对象被销毁的时候 会将_instance设置为null
            _instance = null;
        }
    }
}