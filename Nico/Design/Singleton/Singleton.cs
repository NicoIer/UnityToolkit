using System;
using Nico.Exception;

namespace Nico.Design
{
    /// <summary>
    /// 通用单例模式 它是线程安全的 且会全局存在一份
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> : ISingleton where T : Singleton<T>, new()
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

                        if (_instance == null)
                        {
                            _instance = System.Activator.CreateInstance<T>();
                        }
                    }
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

        public static void Init()
        {
            if (_instance != null)
                throw new SingletonException($"{typeof(T)}already has an instance!!!");
            _instance = new T();
        }

        public static void Dispose()
        {
            _instance = null;
        }
    }
}