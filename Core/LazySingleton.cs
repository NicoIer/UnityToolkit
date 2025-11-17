// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using System;

namespace UnityToolkit
{
    public class LazySingleton<T> : IDisposable where T : new()
    {
        private static readonly Lazy<T> s_instance = new Lazy<T>(() =>
        {
            T t = new T();
            if (t is IOnInit init)
            {
                init.OnInit();
            }

            return t;
        });

        public static T Singleton => s_instance.Value;

        public virtual void Dispose()
        {
            if (Singleton is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}