using System;

namespace Capabilities
{
    public abstract class CapabilityBase<TTag, TOwner> : ICapability<TTag, TOwner>
    {
        // public List<ETag> tags { get; protected set; }

        public ETickGroup tickGroup
        {
            get => _tickGroup;
            set
            {
                if (_baseSetupDone)
                {
                    throw new InvalidOperationException(
                        $"Capability {GetType().Name} TickGroup cannot be changed after Setup!");
                }

                _tickGroup = value;
            }
        }

        private ETickGroup _tickGroup = ETickGroup.Gameplay;
        public uint tickGroupOrder { get; protected set; } = 0;
        public TOwner Owner { get; set; }
        public ICapabilityHolder<TTag, TOwner> capabilityComp { get; protected set; }
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public bool active { get; set; }
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public float activeDuration { get; set; }
        
#if UNITY_EDITOR && ODIN_INSPECTOR
        [Sirenix.OdinInspector.ShowInInspector]
#endif
        public float deActiveDuration { get; set; }

        private bool _baseSetupDone = false;

        public virtual void Setup(ICapabilityHolder<TTag, TOwner> holder, ICapabilitySystem system)
        {
#if UNITY_2017_1_OR_NEWER
            UnityEngine.Assertions.Assert.IsFalse(_baseSetupDone,
                $"Capability {GetType().Name} Setup called multiple times!");
#endif

            system.Register(this);
            Owner = holder.GetOwner();
            capabilityComp = holder;
            _baseSetupDone = true;
        }

        public virtual void OnOwnerDestroyed(ICapabilitySystem system)
        {
            if (active) OnDeactivated();
            system.Unregister(this);
        }

        public abstract bool ShouldActivate();

        public abstract bool ShouldDeactivate();

        public abstract void OnActivated();

        public abstract void OnDeactivated();

        public abstract void TickActive(in float deltaTime);
    }
}