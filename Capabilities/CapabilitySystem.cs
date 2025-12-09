using System;
using System.Collections.Generic;
using UnityToolkit;

namespace Capabilities
{
    public class CapabilitySystem :  ICapabilitySystem
    {

        Dictionary<ETickGroup, List<ICapability>> capabilitiesByTickGroup =
            new Dictionary<ETickGroup, List<ICapability>>();

        private static readonly Comparison<ICapability> _capabilityComparison =
            (a, b) => a.tickGroupOrder.CompareTo(b.tickGroupOrder);

        ETickGroup[] sortedTickGroups;

        public void OnInit()
        {
            sortedTickGroups = EnumHelper<ETickGroup>.keys.DeepCopy();
        }

        public void OnDispose()
        {
        }

        public void Update(in float deltaTime)
        {
            foreach (var group in sortedTickGroups)
            {
                // if (group == ETickGroup.Physics) continue; // FixedUpdate处理Physics组
                if (capabilitiesByTickGroup.TryGetValue(group, out var list))
                {
                    foreach (var capability in list)
                    {
                        bool currentActive = capability.active;
                        if (currentActive && capability.ShouldDeactivate())
                        {
                            capability.active = false;
                            capability.OnDeactivated();
                        }

                        if (!currentActive && capability.ShouldActivate())
                        {
                            capability.active = true;
                            capability.OnActivated();
                        }

                        if (capability.active) // 状态没有变化
                        {
                            capability.activeDuration += deltaTime;
                            capability.deActiveDuration = 0;
                            capability.TickActive(deltaTime);
                        }
                        else
                        {
                            capability.activeDuration = 0;
                            capability.deActiveDuration += deltaTime;
                        }
                    }
                }
            }
        }

        public void FixedUpdate(in float fixedDeltaTime)
        {
            foreach (var group in sortedTickGroups)
            {
                if (capabilitiesByTickGroup.TryGetValue(group, out var list))
                {
                    foreach (var capability in list)
                    {
                        if (!capability.active) continue;
                        if (capability is IPhysicsTick physicsTick)
                        {
                            physicsTick.PhysicsTickActive(fixedDeltaTime);
                        }
                    }
                }
            }
        }

        public void Register(ICapability capability)
        {
            if (!capabilitiesByTickGroup.TryGetValue(capability.tickGroup, out var list))
            {
                list = new List<ICapability>();
                capabilitiesByTickGroup[capability.tickGroup] = list;
            }

            list.Add(capability);
            // Sort
            list.Sort(_capabilityComparison);
        }

        public void Unregister(ICapability capability)
        {
            if (capabilitiesByTickGroup.TryGetValue(capability.tickGroup, out var list))
            {
                list.Remove(capability);
            }
        }
    }
}