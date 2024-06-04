using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityToolkit
{
    public static class Extensions
    {
        internal static string GetMethodName(this Delegate func)
        {
            return func.Method.Name;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void CopyTo<T>(this IEnumerable<T> source, List<T> destination)
        {
            destination.AddRange(source);
        }

        /// <summary>
        /// 这个是消息通信的基础 需要版本不同编程语言 不同平台下 对同一个字符串的hash值都是一样的
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetStableHash(this string str)
        {
            unchecked
            {
                int hash = 23;
                foreach (char c in str)
                    hash = hash * 31 + c;
                return hash;
            }
        }

        public static string ToHexString(this ArraySegment<byte> segment) =>
            BitConverter.ToString(segment.Array, segment.Offset, segment.Count);
        
        
                public static string Green(this string s) => s.SetColor(TextColor.Green);
        public static string White(this string s) => s.SetColor(TextColor.White);
        public static string Purple(this string s) => s.SetColor(TextColor.Purple);
        public static string Yellow(this string s) => s.SetColor(TextColor.Yellow);
        public static string Orange(this string s) => s.SetColor(TextColor.Orange);
        public static string Pink(this string s) => s.SetColor(TextColor.Pink);
        public static string Blue(this string s) => s.SetColor(TextColor.Blue);
        public static string Red(this string s) => s.SetColor(TextColor.Red);
        public static string Sky(this string s) => s.SetColor(TextColor.Sky);



        /// <summary>
        /// 为字符串添加HTML格式的颜色标签
        /// </summary>
        /// <param name="s"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string SetColor(this string s, TextColor type)
        {
            return type switch
            {
                TextColor.White => "<color=#FFFFFF>" + s + "</color>",
                TextColor.Yellow => "<color=#FFFF00>" + s + "</color>",
                TextColor.Sky => "<color=#00FFFF>" + s + "</color>",
                TextColor.Purple => "<color=#FF00AA>" + s + "</color>",
                TextColor.Orange => "<color=#FFAA00>" + s + "</color>",
                TextColor.Red => "<color=#FF0000>" + s + "</color>",
                TextColor.Blue => "<color=#00CCFF>" + s + "</color>",
                TextColor.Green => "<color=#00FF00>" + s + "</color>",
                TextColor.Pink => "<color=#FFAACC>" + s + "</color>",
                _ => s
            };
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TElement Random<TElement>(this TElement[] array)
        {
            if (array == null || array.Length <= 0)
            {
                return default;
            }

            return array[UnityEngine.Random.Range(0, array.Length)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TElement Random<TElement>(this IList<TElement> list)
        {
            if (list == null || list.Count <= 0)
            {
                return default;
            }

            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveTarget<T>(this List<T> list, Func<T, bool> condition)
        {
            int startPos = 0;
            int endPos = 0;
            while (startPos < list.Count && endPos < list.Count)
            {
                while (startPos < list.Count && !condition(list[startPos]))
                {
                    startPos++;
                }

                endPos = startPos;
                while (endPos < list.Count && condition(list[endPos]))
                {
                    endPos++;
                }

                if (startPos < list.Count && endPos < list.Count)
                {
                    list[startPos] = list[endPos];
                    list[endPos] = default;
                }
            }

            list.RemoveRange(startPos, list.Count - startPos);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void RemoveNull<T>(this List<T> list) where T : class
        {
            list.RemoveTarget(e => e == null);
        }
    }
}