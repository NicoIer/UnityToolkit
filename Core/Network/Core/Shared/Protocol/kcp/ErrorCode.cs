// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
// kcp specific error codes to allow for error switching, localization,
// translation to Mirror errors, etc.
namespace kcp2k
{
    public enum ErrorCode : byte
    {
        DnsResolve,       // failed to resolve a host name
        Timeout,          // ping timeout or dead link
        Congestion,       // more messages than transport / network can process
        InvalidReceive,   // recv invalid packet (possibly intentional attack)
        InvalidSend,      // user tried to send invalid data
        ConnectionClosed, // connection closed voluntarily or lost involuntarily
        Unexpected        // unexpected error / exception, requires fix.
    }
}