// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using UnityToolkit;

namespace Network.Server
{
    public interface IServerSystem : ISystem, IOnInit<NetworkServer>
    {
    }
}