using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UnityToolkit
{
    public static class Util
    {
        public static string PrettyBytes(long bytes)
        {
            // bytes
            if (bytes < 1024)
            {
                return $"{bytes} B";
            }
            // kilobytes

            if (bytes < 1024L * 1024L)
            {
                return $"{(bytes / 1024f):F2} KB";
            }
            // megabytes

            if (bytes < 1024 * 1024L * 1024L)
            {
                return $"{(bytes / (1024f * 1024f)):F2} MB";
            }

            // gigabytes
            return $"{(bytes / (1024f * 1024f * 1024f)):F2} GB";
        }

        // pretty print seconds as hours:minutes:seconds(.milliseconds/100)s.
        // double for long running servers.
        public static string PrettySeconds(double seconds)
        {
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            string res = "";
            if (t.Days > 0) res += $"{t.Days}d";
            if (t.Hours > 0) res += $"{(res.Length > 0 ? " " : "")}{t.Hours}h";
            if (t.Minutes > 0) res += $"{(res.Length > 0 ? " " : "")}{t.Minutes}m";
            // 0.5s, 1.5s etc. if any milliseconds. 1s, 2s etc. if any seconds
            if (t.Milliseconds > 0) res += $"{(res.Length > 0 ? " " : "")}{t.Seconds}.{(t.Milliseconds / 100)}s";
            else if (t.Seconds > 0) res += $"{(res.Length > 0 ? " " : "")}{t.Seconds}s";
            // if the string is still empty because the value was '0', then at least
            // return the seconds instead of returning an empty string
            return res != "" ? res : "0s";
        }

        public static void Create(string path)
        {
            string folderPath = System.IO.Path.GetDirectoryName(path);
            if (!System.IO.Directory.Exists(folderPath) && folderPath != null)
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            if (File.Exists(path)) return;
            System.IO.File.Create(path).Dispose();
        }

        public static void Write(string path, string text)
        {
            if (!System.IO.File.Exists(path))
            {
                Create(path);
            }

            System.IO.File.WriteAllText(path, text);
        }

        public static void ReplaceContent(string content, string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Create(path);
            }

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(content);
            }
        }

        public static bool TryReadAllText(string path, out string str)
        {
            str = null;
            if (!System.IO.File.Exists(path)) return false;
            str = System.IO.File.ReadAllText(path);
            return true;
        }
        
        
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