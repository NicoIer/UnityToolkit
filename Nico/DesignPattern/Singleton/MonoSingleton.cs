using System;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;

namespace Nico
{
    /// <summary>
    /// 基于MonoBehaviour的单例模式 仅场景内单例 不会跨场景 切换场景会被销毁
    /// 这个单例是线程安全的 
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour, IMonoSingleton where T : MonoSingleton<T>
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
                                Debug.Log($"can not find MonoSingleton<{typeof(T).Name}> auto create one");
                                GameObject obj = new GameObject(typeof(T).Name);
                                _instance = obj.AddComponent<T>();
                            }
                        }
                    }
                }

                return _instance;
            }
        }

        /// <summary>
        /// 在Disable时访问的途径 之所以会有这个存在 是因为游戏退出时 单例应该被销毁 此时其他的途径访问单例对象 都会造成错误
        /// </summary>
        /// <returns></returns>
        [CanBeNull]
        public static T GetInstanceUnSafe(bool throwError = false)
        {
            if(_instance==null)
            {
                if (throwError)
                {
                    throw new Exception("Application is quitting !!!!");
                }

                return null;
            }

            return Instance;
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
                return;
            }
        }


        protected virtual void OnDestroy()
        {
            //当单例对象被销毁的时候 会将_instance设置为null
            //如何保证单例对象是最后被销毁的呢
            _instance = null;
        }
    }
}