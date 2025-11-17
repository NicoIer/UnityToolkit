// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using System;
using System.Collections.Generic;

namespace Capabilities
{
    public interface ICapabilitySystem
    {
        public void Register(ICapability capability);
        public void Unregister(ICapability capability);
    }

    public interface IPhysicsTick
    {
        void PhysicsTickActive(in float deltaTime);
    }
    
#if UNITY_2017_1_OR_NEWER
    public interface IAnimationMove
    {
        public bool active { get; set; }
        void OnAnimationMove(in UnityEngine.Vector3 deltaPosition, in UnityEngine.Quaternion deltaRotation);
    }

#endif

    public interface ICapability
    {
        float activeDuration { get; set; }

        float deActiveDuration { get; set; }
        bool active { get; set; }
        public ETickGroup tickGroup { get; }

        public uint tickGroupOrder { get; }
        bool ShouldActivate();
        bool ShouldDeactivate();

        void OnActivated();
        void OnDeactivated();

        void TickActive(in float deltaTime);
    }

    public interface ICapability<TTag, TOwner> : ICapability
    {
        TOwner Owner { get; set; }

        ICapabilityHolder<TTag, TOwner> capabilityComp { get; }


        void Setup(ICapabilityHolder<TTag, TOwner> holder, ICapabilitySystem system)
        {
            system.Register(this);
            Owner = holder.GetOwner();
        }


        void OnOwnerDestroyed(ICapabilitySystem system)
        {
            if (active) OnDeactivated();
            system.Unregister(this);
        }
    }


    public interface IComponent
    {
    }

    public interface IConfig
    {
    }


    public interface ICapabilityHolder<in TTag, out TOwner>
    {
        void BlockCapabilities(TTag tag, Instigator instigator);
        void UnblockCapabilities(TTag tag, Instigator instigator);
        bool HasBlockedTag(TTag tag);
        bool TryGetComp<T>(out T component) where T : IComponent;
        bool TryGetConfig<T>(out T config) where T : IConfig;
        TOwner GetOwner();
    }


}