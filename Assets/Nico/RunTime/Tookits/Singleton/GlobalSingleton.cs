using UnityEngine;

namespace Nico
{
    /// <summary>
    /// 全局单例 不会随着场景切换被销毁
    /// 它是线程安全的
    /// </summary>
    public abstract class GlobalSingleton<T> : MonoBehaviour, ISingleton where T : GlobalSingleton<T>
    {
        private static readonly object _lock = typeof(T);
        private static T _instance;

        public static T InstanceNullable => _instance;

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
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this) //这一步比较很重要
            {
                //如果已经被访问过了 代表已经有一个对应的单例对象存在了 那么就会在Awake中销毁自己
                Destroy(gameObject);
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
