// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
#if UNITY_2017_1_OR_NEWER
using UnityEngine;

namespace Capabilities
{
    public abstract class CapabilityAsset : ScriptableObject
    {
        public abstract ICapability[] GetDependencies();
    }
}
#endif