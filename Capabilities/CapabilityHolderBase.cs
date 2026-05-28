// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using System;
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

        protected Dictionary<Type, ICapability> _dictCapabilities;

        private TOwner _owner;

        public CapabilityHolderBase(TOwner owner)
        {
            _owner = owner;
        }

        public CapabilityHolderBase()
        {
        }

        public void Initialize(TOwner owner, List<ICapability<TTag, TOwner>> capabilityList, List<IComponent> componentList, ICapabilitySystem system)
        {
            _owner = owner;
            capabilities = capabilityList.ToArray();
            components = componentList.ToArray();
            configs = Array.Empty<IConfig>();

            _dictCapabilities = new Dictionary<Type, ICapability>();
            foreach (var capability in capabilities)
            {
                if (capability is ICapability<TTag, TOwner> typed)
                {
                    typed.Setup(this, system);
                }
                _dictCapabilities.Add(capability.GetType(), capability);
            }
        }

        public bool TryGetCapability<T>(out T capability) where T : ICapability
        {
            Type type = typeof(T);
            if (_dictCapabilities != null && _dictCapabilities.TryGetValue(type, out var cap))
            {
                capability = (T)cap;
                return true;
            }
            capability = default;
            return false;
        }

        public bool TryAddCapability<T>(T capability, ICapabilitySystem system) where T : ICapability<TTag, TOwner>
        {
            Type type = typeof(T);
            if (_dictCapabilities != null && !_dictCapabilities.ContainsKey(type))
            {
                capability.Setup(this, system);
                _dictCapabilities.Add(type, capability);
                return true;
            }
            return false;
        }

        public void OnOwnerDestroyed(ICapabilitySystem system)
        {
            if (capabilities == null) return;
            foreach (var capability in capabilities)
            {
                if (capability is ICapability<TTag, TOwner> typed)
                {
                    typed.OnOwnerDestroyed(system);
                }
            }
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
            if (components == null) return false;
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
            if (configs == null) return false;
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

        public bool IsCapabilityActive<TCapability>() where TCapability : ICapability
        {
            foreach (var capability in capabilities)
            {
                if (capability is TCapability tCapability)
                {
                    return tCapability.active;
                }
            }

            return false;
        }
    }
}