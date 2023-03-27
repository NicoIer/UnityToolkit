using System;
using JetBrains.Annotations;
using Nico.Design;
using Nico.Exception;
using Unity.Netcode;
using UnityEngine;

namespace Nico.Network
{
    /// <summary>
    /// 网络Mono单例模式
    /// 所有客户端上都只会有一个实例
    /// 仅场景内单例 不会跨场景 切换场景会被销毁
    /// 线程安全
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NetSingleton<T> : NetworkBehaviour, ISingleton where T : NetSingleton<T>
    {
        private static T _instance;
        private static readonly object _lock = typeof(T);

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
                                // //找不见 就 new 一个
                                // GameObject obj = new GameObject(typeof(T).Name);
                                // //网络物体需要添加NetworkObject组件
                                // obj.AddComponent<NetworkObject>();
                                // _instance = obj.AddComponent<T>();
                                throw new SingletonException("can find " + typeof(T).Name + " in scene");
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
                return;
            }
        }

        protected virtual void OnEnable()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (_instance == this)
            {
                _instance = null;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            if (_instance == null)
            {
                _instance = this as T;
            }
        }
    }
}