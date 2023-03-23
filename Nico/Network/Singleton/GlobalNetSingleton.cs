using System;
using Nico.Design;
using Nico.Exception;
using Unity.Netcode;
using UnityEngine;

namespace Nico.Network
{
    public class GlobalNetSingleton<T> : NetworkBehaviour, ISingleton where T : GlobalNetSingleton<T>
    {
        private static readonly object _lock = typeof(T);
        private static T _instance;

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
                                throw new SingletonException($"{typeof(T)} can not find in scene");
                            }
                        }
                    }
                }

                DontDestroyOnLoad(_instance.gameObject);
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            //如果Awake前没有被访问 那么就会在Awake中初始化
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this) //这一步比较很重要
            {
                //如果已经被访问过了 代表已经有一个对应的单例对象存在了 那么就会在Awake中销毁自己
                Destroy(this);
            }
        }

        protected void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(this);
            }
            else if (_instance != this)
            {
                Destroy(this);
            }
        }

        public override void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }

        public override void OnNetworkDespawn()
        {
            if (_instance == this)
                _instance = null;
        }

        public override void OnNetworkSpawn()
        {
            if (_instance != this)
            {
                Destroy(this);
            }
        }
    }
}