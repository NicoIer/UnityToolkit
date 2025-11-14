#if UNITY_2017_1_OR_NEWER
using System.Collections.Generic;
using UnityEngine;

namespace Capabilities
{
    public abstract class ConfigAsset : ScriptableObject
    {
        public abstract IConfig[] GetDependencies();
    }
}
#endif