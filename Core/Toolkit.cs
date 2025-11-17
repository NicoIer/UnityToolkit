// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using System;
using System.Threading;

namespace UnityToolkit
{
    public static class Toolkit
    {
        public static uint IncreaseId(uint current)
        {
#if NET8_0
            Interlocked.Increment(ref current);
#else
            long cur = current; // uint 转long 不会丢失精度
            Interlocked.Increment(ref cur); // 如果是NetStandard2.0 则用long进行增加
            current = (uint)cur; // 然后再转回uint
#endif
            return current;
        }
    }

    public static class EnumHelper<T>
    {
        public static readonly T[] keys = (T[])Enum.GetValues(typeof(T));
    }
}