// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
namespace Network
{
    // public enum NetworkAuthority : byte
    // {
    //     ServerOnly,
    //     ClientOnly,
    //     Both
    // }

    public enum SyncMode
    {
        Observers,
        Owner
    }

    public enum SyncDirection
    {
        ServerToClient,
        ClientToServer
    }
}