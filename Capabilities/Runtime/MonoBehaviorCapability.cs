// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
#if UNITY_2017_1_OR_NEWER
using UnityEngine;

namespace Capabilities
{
    public abstract class MonoBehaviorCapability<TTag, TOwner> : MonoBehaviour, ICapability<TTag, TOwner>
    {
        // public List<ETag> tags { get; protected set; }
        public ETickGroup tickGroup { get; protected set; }
        public uint tickGroupOrder { get; protected set; }
        public TOwner Owner { get; set; }
        public ICapabilityHolder<TTag, TOwner> capabilityComp { get; protected set; }
        public bool active { get; set; }
        public float activeDuration { get; set; }
        public float deActiveDuration { get; set; }
        public abstract bool ShouldActivate();

        public abstract bool ShouldDeactivate();

        public abstract void OnActivated();

        public abstract void OnDeactivated();

        public abstract void TickActive(in float deltaTime);
    }
}
#endif