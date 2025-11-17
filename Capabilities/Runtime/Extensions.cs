// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.


#if UNITY_2017_1_OR_NEWER

using System.Collections.Generic;

namespace Capabilities
{
    public static class Extensions
    {
        public static IConfig[] MergeConfigs(this IConfig[] baseConfigs, ConfigAsset[] additionalConfigs)
        {
            List<IConfig> mergedConfigs = new List<IConfig>(baseConfigs);
            foreach (var configAsset in additionalConfigs)
            {
                var dependencies = configAsset.GetDependencies();
                mergedConfigs.AddRange(dependencies);
            }

            return mergedConfigs.ToArray();
        }

        public static IComponent[] MergeComponents(this IComponent[] baseComponents,
            ComponentAsset[] additionalComponents)
        {
            List<IComponent> mergedComponents = new List<IComponent>(baseComponents);
            foreach (var componentAsset in additionalComponents)
            {
                var dependencies = componentAsset.GetDependencies();
                mergedComponents.AddRange(dependencies);
            }

            return mergedComponents.ToArray();
        }

        public static ICapability[] MergeCapabilities(this ICapability[] baseCapabilities,
            CapabilityAsset[] additionalCapabilities)
        {
            List<ICapability> mergedCapabilities = new List<ICapability>(baseCapabilities);
            foreach (var capabilityAsset in additionalCapabilities)
            {
                var dependencies = capabilityAsset.GetDependencies();
                mergedCapabilities.AddRange(dependencies);
            }

            return mergedCapabilities.ToArray();
        }
    }
}
#endif