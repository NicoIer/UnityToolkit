using System;
using System.Collections.Generic;
using System.Reflection;

namespace Nico
{
    public static class ReflectionUtils
    {
        private static readonly AppDomain _appDomain = AppDomain.CurrentDomain;
        private static readonly Assembly[] _assemblies = _appDomain.GetAssemblies();

        public static IEnumerable<Type> GetTypesWithAttribute<T>() where T : Attribute
        {
            foreach (var assembly in _assemblies)
            {
                //找到所有被T特性标记的类型
                var types = assembly.GetTypes();
                foreach (var t in types)
                {
                    if (t.GetCustomAttribute<T>() != null)
                    {
                        yield return t;
                    }
                }
            }
        }

        public static IEnumerable<Type> GetTypesWithInterface<T>(bool skipAbstract = true, bool skipInterface = true)
        {
            var type = typeof(T);
            if (!type.IsInterface)
            {
                throw new ArgumentException($"T:[{type}] must be an interface ");
            }
            //拿到_appDomain中所有的程序集

            //遍历所有的程序集 拿到实现了T接口的所有类型
            foreach (var assembly in _assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var t in types)
                {
                    if (t.IsAbstract && skipAbstract) continue;
                    if (t.IsInterface && skipInterface) continue;
                    if (t.GetInterface(type.FullName) != null)
                    {
                        yield return t;
                    }
                }
            }
        }

        public static bool IsStruct(this Type type)
        {
            return !type.IsPrimitive && !type.IsEnum && type.IsValueType;
        }
        
        public static bool IsList(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }
        
    }
}