using System;
using System.Collections.Generic;

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

        public StateMachine(T owner, params IState<T>[] states)
        {
            Owner = owner;
            States = new();
            foreach (var state in states)
            {
                States.Add(state.GetType(), state);
            }
        }

        public void Start<TState>() where TState : IState<T>
        {
            if (CurrentState != null)
                throw new DesignException("状态机已经启动");
            States[typeof(TState)].Enter();
        }

        public void AddState(IState<T> state)
        {
            if (!States.ContainsKey(state.GetType()))
            {
                States.Add(state.GetType(), state);
            }
        }


        public void ChangeState<TState>() where TState : IState<T>
        {
            CurrentState?.Exit();
            CurrentState = States[typeof(TState)];
            CurrentState?.Enter();
        }


        public void Update()
        {
            CurrentState?.Update();
        }
    }
}