using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityToolkit
{
#if UNITY_EDITOR
    [Serializable]
#endif
    public class StateMachine<TOwner>
    {
        public TOwner Owner { get; private set; }
#if UNITY_EDITOR
        [field: UnityEngine.SerializeReference]
#endif
        public State<TOwner> CurrentState { get; protected set; }

        protected Dictionary<Type, State<TOwner>> stateDic;

        public StateMachine(TOwner owner)
        {
            stateDic = new Dictionary<Type, State<TOwner>>();
            Owner = owner;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Get<TState>(out TState state) where TState : State<TOwner>
        {
            if (stateDic.TryGetValue(typeof(TState), out var value))
            {
                state = (TState)value;
                return true;
            }

            state = null;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<T>() where T : State<TOwner>
        {
            CurrentState = stateDic[typeof(T)];
            CurrentState.OnEnter(Owner);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Change<T>() where T : State<TOwner>
        {
            CurrentState.OnExit(Owner);
            CurrentState = stateDic[typeof(T)];
            CurrentState.OnEnter(Owner);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(T state) where T : State<TOwner>
        {
            stateDic.Add(typeof(T), state);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void OnUpdate()
        {
            CurrentState.OnUpdate(Owner);
        }
    }
}