using System;

namespace Nico.Design
{
    /// <summary>
    /// 通用单例模式 它是线程安全的 且会全局存在一份
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> : ISingleton,IInitializable, IDisposable where T : Singleton<T>, new()
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
                            _instance = new T();
                        }
                        
                    }
                    _instance.Init();
                }

                return _instance;
            }
        }

        /// <summary>
        /// 隐藏自身构造函数
        /// </summary>
        protected Singleton()
        {
        }


        /// <summary>
        /// 单例资源释放函数
        /// </summary>
        public virtual void Dispose()
        {
            _instance = null;
        }

        /// <summary>
        /// 单例初始化函数
        /// </summary>
        /// <exception cref="DesignException"></exception>
        public virtual void Init()
        {
            if (_instance != null)
                throw new DesignException($"{typeof(T)}already has an instance!!!");
            _instance = new T();
        }
    }
}