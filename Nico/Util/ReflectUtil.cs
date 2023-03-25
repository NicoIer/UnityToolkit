using System;
using System.Collections.Generic;
using System.Linq;

namespace Nico.Util
{
    /// <summary>
    /// C# 反射工具类
    /// </summary>
    public static class ReflectUtil
    {
        public static IEnumerable<Type> GetTypesByInterface<T>(AppDomain domain)
        {
            return domain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => t.GetInterfaces().Contains(typeof(T)));
        }
    }
}