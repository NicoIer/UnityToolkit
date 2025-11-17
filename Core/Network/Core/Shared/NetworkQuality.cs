// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
namespace Network
{
    public enum NetworkQuality : byte
    {
        Excellent, // 1-30ms
        Good, // 31-60ms
        Normal, // 61-100ms
        Poor, // 101-200ms
        Bad, // >200ms
    }
}