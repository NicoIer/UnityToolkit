// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using System;

namespace Network.Client
{
    public interface IClientHandler
    {
        void OnConnected();
        void OnDataReceived(ArraySegment<byte> data);
        void OnDisconnected();
        void OnDataSent(ArraySegment<byte> data);
        
        void OnUpdate(in float deltaTime);
    }
}