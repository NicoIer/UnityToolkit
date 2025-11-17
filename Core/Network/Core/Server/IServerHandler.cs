// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using System;

namespace Network.Server
{
    public interface IServerHandler
    {
        void OnConnected(int connectionId);
        void OnDataReceived(int connectionId, ArraySegment<byte> data);
        void OnDisconnected(int connectionId);
        void OnDataSent(int connectionId, ArraySegment<byte> data);

        void OnUpdate(float deltaTime);
    }
}