using System;
using System.Collections.Generic;

namespace Nico.Design
{
    /// <summary>
    /// 状态机接口
    /// </summary>
    /// <typeparam name="TOwner">状态机持有者</typeparam>
    public interface IStateMachine<TOwner> where TOwner : class
    {
        public TOwner Owner { get; }
        public Dictionary<Type, IState<TOwner>> States { get; }
        public IState<TOwner> CurrentState { get; }

        public void Start<TState>() where TState : IState<TOwner>;

        // public void AddState<TState>() where TState : IState<TOwner>;
        public void AddState(IState<TOwner> state);
        public void ChangeState<TState>() where TState : IState<TOwner>;
        public void Update();
    }
}