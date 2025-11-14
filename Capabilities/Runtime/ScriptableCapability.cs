#if UNITY_2017_1_OR_NEWER
using System.Collections.Generic;
using UnityEngine;

namespace Capabilities
{
    public abstract class ScriptableCapability<TTag, TOwner> : ScriptableObject, ICapability<TTag, TOwner>
    {
        public abstract bool ShouldActivate();

        public abstract bool ShouldDeactivate();

        public abstract void OnActivated();

        public abstract void OnDeactivated();

        public abstract void TickActive(in float deltaTime);


        // public List<ETag> tags { get; protected set; }
        public ETickGroup tickGroup { get; protected set; }
        public uint tickGroupOrder { get; protected set; }
        public TOwner Owner { get; set; }
        public ICapabilityHolder<TTag, TOwner> capabilityComp { get; protected set; }
        public bool active { get; set; }
        public float activeDuration { get; set; }
        public float deActiveDuration { get; set; }
    }
}
#endif