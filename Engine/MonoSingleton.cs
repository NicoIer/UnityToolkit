using UnityEngine;

namespace UnityToolkit
{
    public interface IAutoCreateSingleton
    {
    }

    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _singleton;

        protected virtual bool dontDestroyOnLoad()
        {
            return false;
        }

        public static T SingletonNullable => _singleton;

        public static T Singleton
        {
            get
            {
                if (Application.isPlaying == false)
                {
                    return null;
                }

                if (_singleton != null) return _singleton; //第一次访问
                _singleton = FindObjectOfType<T>(); // 从场景中查找
                if (_singleton != null)
                {
                    _singleton.OnSingletonInit(); //手动初始化
                    return _singleton;
                }

                if (typeof(T).GetInterface(nameof(IAutoCreateSingleton)) != null)
                {
                    GameObject go = new GameObject(typeof(T).Name);
                    _singleton = go.AddComponent<T>();
                    _singleton.OnSingletonInit();
                    return _singleton;
                }

                throw new System.NullReferenceException(
                    $"Singleton<{typeof(T).Name}>.Singleton -> {typeof(T).Name} is null");
            }
        }

        private void OnSingletonInit()
        {
            // Debug.Log($"Singleton<{typeof(T).Name}>.OnInit() -> {gameObject.name}");
            transform.SetParent(null);
            if (dontDestroyOnLoad())
            {
                DontDestroyOnLoad(gameObject);
            }

            OnInit();
        }


        protected virtual void Awake()
        {
            //Awake时如果还没有被访问 则将自己赋值给_singleton
            if (_singleton == null)
            {
                _singleton = this as T;
                // Debug.Log($"Singleton<{typeof(T).Name}>.Awake() -> {gameObject.name}");
                _singleton.OnSingletonInit();
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void ResetStatic()
        {
            _singleton = null;
        }
#endif
    }
}