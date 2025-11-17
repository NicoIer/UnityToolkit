// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using System.Collections.Generic;


namespace Capabilities
{
    public class CapabilityHolderBase<TTag, TOwner> : ICapabilityHolder<TTag, TOwner>
    {
        // Runtime  
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        protected Dictionary<TTag, List<Instigator>> tagBlockers = new Dictionary<TTag, List<Instigator>>();

#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        protected ICapability[] capabilities;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        protected IComponent[] components;

#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        protected IConfig[] configs;


        private readonly TOwner _owner;

        public CapabilityHolderBase(TOwner owner)
        {
            _owner = owner;
        }

        public void BlockCapabilities(TTag tag, Instigator instigator)
        {
            if (!tagBlockers.ContainsKey(tag))
            {
                tagBlockers[tag] = new List<Instigator>();
            }

            if (!tagBlockers[tag].Contains(instigator))
            {
                tagBlockers[tag].Add(instigator);
            }
        }

        public void UnblockCapabilities(TTag tag, Instigator instigator)
        {
            if (tagBlockers.ContainsKey(tag))
            {
                tagBlockers[tag].Remove(instigator);
                if (tagBlockers[tag].Count == 0)
                {
                    tagBlockers.Remove(tag);
                }
            }
        }

        public bool HasBlockedTag(TTag tag)
        {
            return tagBlockers.ContainsKey(tag) && tagBlockers[tag].Count > 0;
        }

        public bool TryGetComp<T>(out T component) where T : IComponent
        {
            component = default(T);
            foreach (var comp in components)
            {
                if (comp is T tComp)
                {
                    component = tComp;
                    return true;
                }
            }

#if UNITY_2017_1_OR_NEWER
            UnityEngine.Debug.LogError("TryGetComp failed for type: " + typeof(T).Name);
#endif
            return false;
        }

        public bool TryGetConfig<T>(out T config) where T : IConfig
        {
            config = default(T);
            foreach (var conf in configs)
            {
                if (conf is T tConfig)
                {
                    config = tConfig;
                    return true;
                }
            }
#if UNITY_2017_1_OR_NEWER
            UnityEngine.Debug.LogError("TryGetConfig failed for type: " + typeof(T).Name);
#endif
            return false;
        }

        public TOwner GetOwner()
        {
            return _owner;
        }
    }
}