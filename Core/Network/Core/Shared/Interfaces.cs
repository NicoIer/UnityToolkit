// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
namespace Network
{
    public interface INetworkMessage
    {
        public const int IdSize = sizeof(ushort);
    }
}