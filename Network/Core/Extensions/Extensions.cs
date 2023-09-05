using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Nico
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
    }
}