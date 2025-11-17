// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
namespace kcp2k
{
    // channel type and header for raw messages
    public enum KcpChannel : byte
    {
        // don't react on 0x00. might help to filter out random noise.
        Reliable   = 1,
        Unreliable = 2
    }
}