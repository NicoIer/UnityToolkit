using System;
using System.Collections.Generic;
using UnityEngine;

namespace Nico.Design
{
    /// <summary>
    /// 通用状态机
    /// </summary>
    /// <typeparam name="T">状态机持有者</typeparam>
    public class StateMachine<T> : IStateMachine<T> where T : class
    {
        public T Owner { get; }
        public Dictionary<Type, IState<T>> States { get; protected set; }
        public IState<T> CurrentState { get; protected set; }

        public StateMachine(T owner)
        {
            Owner = owner;
            States = new();
        }
        public StateMachine(T owner, params Type[] statesType)
        {
            Owner = owner;
            States = new();
            foreach (var type in statesType)
            {
                IState<T> state = Activator.CreateInstance(type) as IState<T>;
                state.Init(owner, this);
                States.Add(type, state);
            }
        }

        public StateMachine(T owner, params IState<T>[] states)
        {
            Owner = owner;
            States = new();
            foreach (var state in states)
            {
                state.Init(owner, this);
                States.Add(state.GetType(), state);
            }
        }

        public void Start<TState>() where TState : IState<T>
        {
            if (CurrentState != null)
                throw new DesignException("状态机已经启动");
            CurrentState = States[typeof(TState)];
            CurrentState.Enter();
        }

        public void AddState(IState<T> state)
        {
            if (!States.ContainsKey(state.GetType()))
            {
                States.Add(state.GetType(), state);
            }
        }


        public virtual void ChangeState<TState>() where TState : IState<T>
        {
            CurrentState?.Exit();
            CurrentState = States[typeof(TState)];
            CurrentState?.Enter();
        }


        public virtual void Update()
        {
            CurrentState?.Update();
        }
    }
}