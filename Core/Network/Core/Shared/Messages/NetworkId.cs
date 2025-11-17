// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using System;
using System.Runtime.CompilerServices;

namespace Network
{
    /// <summary>
    /// 比dict更快
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class NetworkId<T>
    {
        public static readonly ushort Value = NetworkId.CalculateId<T>();
    }

    public static class NetworkId
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort CalculateId<T>() => typeof(T).FullName.GetStableHashCode16();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort CalculateId(Type type) => type.FullName.GetStableHashCode16();
    }
}