// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
#if UNITY_2017_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Capabilities
{
    public abstract class MonoBehaviorCapabilityHolder<TTag> : MonoBehaviour, ICapabilityHolder<TTag, GameObject>
    {
        // Editor
        // public CapabilityAsset[] capabilityAssets;,
        public ComponentAsset[] componentAssets;
        public ConfigAsset[] configAssets;
        // public SheetAsset[] sheetAssets;

        // Runtime  
#if  SIRIXIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        protected Dictionary<TTag, List<Instigator>> tagBlockers = new Dictionary<TTag, List<Instigator>>();

     
#if  SIRIXIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        protected ICapability<TTag, GameObject>[] capabilities;

        
#if  SIRIXIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        protected IComponent[] components;

        
#if  SIRIXIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        protected IConfig[] configs;


        public void BlockCapabilities(TTag playerTag, Instigator instigator)
        {
            if (!tagBlockers.ContainsKey(playerTag))
            {
                tagBlockers[playerTag] = new List<Instigator>();
            }

            if (!tagBlockers[playerTag].Contains(instigator))
            {
                tagBlockers[playerTag].Add(instigator);
            }
        }

        public void UnblockCapabilities(TTag playerTag, Instigator instigator)
        {
            if (tagBlockers.ContainsKey(playerTag))
            {
                tagBlockers[playerTag].Remove(instigator);
                if (tagBlockers[playerTag].Count == 0)
                {
                    tagBlockers.Remove(playerTag);
                }
            }
        }

        public bool HasBlockedTag(TTag playerTag)
        {
            return tagBlockers.ContainsKey(playerTag) && tagBlockers[playerTag].Count > 0;
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

            Debug.LogError("TryGetComp failed for type: " + typeof(T).Name);
            return false;
        }

        public bool TryGetConfig<T>(out T config) where T : IConfig
        {
            config = default(T);
            foreach (var conf in configs)
            {
                if (conf is T tConf)
                {
                    config = tConf;
                    return true;
                }
            }

            Debug.LogError("TryGetConfig failed for type: " + typeof(T).Name);
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public GameObject GetOwner()
        {
            return gameObject;
        }
    }
}
#endif