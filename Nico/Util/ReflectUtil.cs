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

        /// <summary>
        /// 在指定程序集中获取所有继承自指定类的类
        /// </summary>
        /// <param name="currentDomain"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static IEnumerable<Type> GetTypesByParentClass<T>(AppDomain currentDomain)
        {
            return currentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
                .Where(t => t.BaseType == typeof(T));
        }
    }
}