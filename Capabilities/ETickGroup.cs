// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using System;

namespace Capabilities
{
    public enum ETickGroup : int
    {
        NetworkEarly,
        Input,
        Gameplay,
        AfterGameplay,
        NetworkLate,
    }
}