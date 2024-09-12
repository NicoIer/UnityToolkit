#if UNITY_5_6_OR_NEWER
using System;
using System.Threading;
using UnityEngine;

namespace UnityToolkit
{
    public interface IOnlyPlayingModelSingleton
    {
    }

    public interface IAutoCreateSingleton
    {
    }

    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _singleton;

        protected virtual bool DontDestroyOnLoad()
        {
            return false;
        }

        public static T SingletonNullable => _singleton;

        public static T Singleton
        {
            get
            {
                // 非主线程访问
                // 如果T是仅在播放模式下的单例
                if (typeof(IOnlyPlayingModelSingleton).IsAssignableFrom(typeof(T)))
                {
                    if (Application.isPlaying == false)
                    {
                        return null;
                    }
                }


                if (_singleton != null) return _singleton; //第一次访问
                _singleton = FindFirstObjectByType<T>(UnityEngine.FindObjectsInactive.Include); // 从场景中查找
                if (_singleton != null)
                {
                    _singleton.OnSingletonInit(); //手动初始化
                    return _singleton;
                }

                if (typeof(IAutoCreateSingleton).IsAssignableFrom(typeof(T)))
                {
                    GameObject go = new GameObject($"{typeof(T).Name}");
                    _singleton = go.AddComponent<T>();
                    _singleton.OnSingletonInit();
                    return _singleton;
                }

                return null;
                // throw new NullReferenceException($"Singleton<{typeof(T).Name}>.Singleton -> {typeof(T).Name} is null");
            }
        }

        private void OnSingletonInit()
        {
            // Debug.Log($"Singleton<{typeof(T).Name}>.OnInit() -> {gameObject.name}");
            transform.SetParent(null);
            if (DontDestroyOnLoad())
            {
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }

            OnInit();
        }


        protected virtual void Awake()
        {
            //Awake时如果还没有被访问 则将自己赋值给_singleton
            if (_singleton == null)
            {
                if (this is T t)
                {
                    _singleton = t;
                    _singleton.OnSingletonInit();
                    return;
                }
                
                // unknown error
                Destroy(gameObject);
                return;
            }

            //如果已经被访问过 则销毁自己
            if (_singleton != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnInit()
        {
        }

        protected virtual void OnDispose()
        {
        }

        private void OnDestroy()
        {
            if (_singleton == this)
            {
                _singleton.OnDispose();
                _singleton = null;
            }
        }


        // Unity 2022 后 生命周期变更 OnApplicationQuit -> OnDisable -> OnDestroy
        private void OnApplicationQuit()
        {
            if (_singleton == this)
            {
                _singleton.OnDispose();
                _singleton = null;
            }
        }

#if UNITY_EDITOR

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatic()
        {
            _singleton = null;
        }
#endif
    }
}
#endif