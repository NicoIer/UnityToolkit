// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
#if UNITY_2017_1_OR_NEWER
using System.Collections.Generic;
using UnityEngine;

namespace Capabilities
{
    public abstract class ComponentAsset : ScriptableObject
    {
        public abstract IEnumerable<IComponent> GetDependencies();
        public abstract int GetDependencyCount();
    }
}
#endif