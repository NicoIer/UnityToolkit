using System;
using JetBrains.Annotations;
using Nico.Exception;
using Unity.VisualScripting;
using UnityEngine;

namespace Nico.Design
{
    /// <summary>
    /// 基于MonoBehaviour的单例模式 仅场景内单例 不会跨场景 切换场景会被销毁
    /// 这个单例是线程安全的 
    /// </summary>
    public abstract class SceneSingleton<T> : MonoBehaviour, ISingleton where T : SceneSingleton<T>
    {
        private static readonly object _lock = typeof(T);
        private static T _instance;

        /// <summary>
        /// 正常访问时的途径
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance is null)
                {
                    //双重检查锁
                    lock (_lock)
                    {
                        if (_instance is null)
                        {
                            _instance = FindObjectOfType<T>(); //从场景中寻找一个T类型的组件
                            if (_instance is null)
                            {
                                throw new SingletonException("Can not find " + typeof(T).Name + " in scene");
                            }
                            _instance.Awake();
                        }
                    }
                }

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            //如果Awake前没有被访问 那么就会在Awake中初始化
            if (_instance == null)
            {
                _instance = this as T;
            }
            else if (_instance != this)
            {
                //如果已经被访问过了 代表已经有一个对应的单例对象存在了 那么就会在Awake中销毁自己
                Destroy(gameObject);
            }
        }

        protected virtual void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}