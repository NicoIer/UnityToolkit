using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nico
{
    public abstract class State<T>
    {
        protected T owner;

        public State(T owner)
        {
            this.owner = owner;
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnExit()
        {
        }
    }

    public class StateMachine<TOwner>// where TOwner : MonoBehaviour
    {
        private TOwner _owner;
        public State<TOwner> currentState { get; protected set; }
        private Dictionary<Type, State<TOwner>> _stateDic = new Dictionary<Type, State<TOwner>>();

        public StateMachine(TOwner owner)
        {
            _owner = owner;
        }

        public bool Get<TState>(out TState state) where TState : State<TOwner>
        {
            if (_stateDic.TryGetValue(typeof(TState), out var value))
            {
                state = (TState)value;
                return true;
            }

            state = null;
            return false;
        }

        public void Start<T>() where T : State<TOwner>
        {
            currentState = _stateDic[typeof(T)];
            currentState.OnEnter();
        }

        public void Change<T>() where T : State<T>
        {
            currentState.OnExit();
            currentState = _stateDic[typeof(T)];
            currentState.OnEnter();
        }

        public void Add<T>(T state) where T : State<TOwner>
        {
            if (_stateDic.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"state:{typeof(T)} is already in state machine");
                return;
            }

            _stateDic.Add(typeof(T), state);
        }

        public void OnUpdate()
        {
            currentState.OnUpdate();
        }
    }
}