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

        private List<ITransition<TOwner>> transitions;

        private Dictionary<int, object> paramDict;

        public StateMachine(TOwner owner)
        {
            stateDic = new Dictionary<Type, State<TOwner>>();
            transitions = new List<ITransition<TOwner>>();
            paramDict = new Dictionary<int, object>();
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
            Change(typeof(T));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void Change(Type type)
        {
            CurrentState?.OnExit(Owner);
            CurrentState = stateDic[type];
            CurrentState.OnEnter(Owner);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add<T>(T state) where T : State<TOwner>
        {
            stateDic.Add(typeof(T), state);
        }

        public void Add<T>() where T : State<TOwner>, new()
        {
            stateDic.Add(typeof(T), new T());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual void OnUpdate()
        {
            if(CurrentState == null) return;
            CurrentState.OnUpdate(Owner);
        }

        public void AddTransition(ITransition<TOwner> transition)
        {
            transitions.Add(transition);
        }

        public void AddTransition<T>() where T : ITransition<TOwner>, new()
        {
            transitions.Add(new T());
        }

        public void RemoveTransition(ITransition<TOwner> transition)
        {
            transitions.Remove(transition);
        }

        public virtual void OnTransition()
        {
            foreach (var transition in transitions)
            {
                if (!transition.GetNext(out var nextState, this, Owner)) continue;
                Change(nextState);
                break;
            }
        }


        public void SetParam(int paramKey, object value)
        {
            paramDict[paramKey] = value;
        }

        public object GetParam(int paramKey)
        {
            return paramDict[paramKey];
        }
    }
}