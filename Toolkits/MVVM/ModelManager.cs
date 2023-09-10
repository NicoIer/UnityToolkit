using System;
using System.Runtime.CompilerServices;

namespace Nico
{
    //如果需要使用架构模式，可以使用这个 每个游戏有唯一的一个架构，然后有各自的ModelManager
    //但是这样的话，每个Model都要有一个泛型参数IGame 来标识这个Model属于哪个游戏
    //<TGame> where TGame:IGame
    // public class ModelManager<TGame> where TGame:IGame
    
    
    
    // 用于全局访问某个Model实现的ModelManager
    public static class ModelManager 
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Get<T>() where T : class,IModel, new()
        {
            if (Models<T>.Instance == null)
            {
                Register<T>();
            }

            return Models<T>.Instance;
        } 

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Save<T>() where T : IModel
        {
            if (Models<T>.Instance == null)
            {
                throw new ArgumentException($"Model:{typeof(T)} is not registered");
                // return;
            }

            Models<T>.Instance.OnSave();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Register<T>(T model) where T : class, IModel
        {
            if (Models<T>.Instance != null)
            {
                throw new ArgumentException($"Model:{typeof(T)} is already registered, will be replaced");
            }

            Models<T>.Instance = model;
            model.OnRegister();

            UnityEngine.Application.quitting -= Models<T>.Instance.OnSave;
            UnityEngine.Application.quitting += Models<T>.Instance.OnSave; 

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Register<T>() where T : class, IModel, new()
        {
            if (Models<T>.Instance != null)
            {
                throw new ArgumentException($"Model:{typeof(T)} is already registered, will be replaced");
            }

            T model = new T();
            Models<T>.Instance = model;
            model.OnRegister();

            UnityEngine.Application.quitting -= OnApplicationQuit<T>;
            UnityEngine.Application.quitting += OnApplicationQuit<T>;

        }

        private static void OnApplicationQuit<T>() where T : IModel
        {
            if (Models<T>.Instance == null)
            {
                return;
            }
            Models<T>.Instance.OnSave();
            Models<T>.Instance = default;
        }
    }

    // faster than dictionary
    internal static class Models<T> where T : IModel
    {
        internal static T Instance;
    }
}