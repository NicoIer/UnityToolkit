using UnityEngine;

namespace Nico
{
    /// <summary>
    /// 基于MonoBehaviour的单例模式 仅场景内单例 不会跨场景 切换场景会被销毁
    /// 这个单例是线程安全的 
    /// </summary>
    public abstract class SceneSingleton<T> : MonoBehaviour, ISingleton where T : SceneSingleton<T>
    {
        private static readonly object _lock = typeof(T);
        private static T _instance;

        public static T InstanceNullable => _instance;
        /// <summary>
        /// 正常访问时的途径
        /// </summary>
        public static T Instance
        {
            get
            {
                if (!Application.isPlaying) return null;
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
                                Debug.LogWarning($"Can not find {typeof(T)} in scene");
                                var obj = new GameObject(typeof(T).ToString());
                                _instance = obj.AddComponent<T>();
                                return _instance;
                            }
                        }
                    }

                    //如果在Awake前被访问 则 Awake将在此处调用一次
                    _instance.Awake();
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
                Destroy(this);
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
                // _destroyed = true;
            }
        }

        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // private static void _clear_singleton()
        // {
        //     _destroyed = false;
        // }
    }
}