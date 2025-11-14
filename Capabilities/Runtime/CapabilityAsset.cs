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