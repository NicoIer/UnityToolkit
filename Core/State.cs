// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace UnityToolkit
{
    public interface IState
    {
    }

    public interface IState<T> : IState
    {
        void OnInit(T owner, IStateMachine<T> stateMachine);
        void OnDispose(T owner, IStateMachine<T> stateMachine);
        void OnEnter(T owner, IStateMachine<T> stateMachine);
        void Transition(T owner, IStateMachine<T> stateMachine);
        void OnUpdate(T owner, IStateMachine<T> stateMachine);
        void OnExit(T owner, IStateMachine<T> stateMachine);
    }


    public interface IStateMachine<TOwner>
    {
        event Action<IState<TOwner>, IState<TOwner>> OnStateChange;
        bool running { get; }
        TOwner owner { get; }
        IState<TOwner> currentState { get; }
        void SetParam<T>(string key, T value);
        T GetParam<T>(string key);
        bool Change<T>() where T : IState<TOwner>;
        bool Change(Type type);
        void Run<T>() where T : IState<TOwner>;
        void Run(Type type);
        void Add<T>(T state) where T : IState<TOwner>;
        void Add<T>() where T : IState<TOwner>, new() => Add(new T());
        void Stop();
        void OnUpdate();
        void RemoveParam(string key);
        bool ContainsParam(string battleSettlementData);
    }

    public interface IBlackboard
    {
        event Action<string> OnRemove;
        event Action<string> OnAdd;
        event Action<string> OnSet;
        T Get<T>(in string key);
        bool TryGetValue(in string key, out object o);
        bool TryGetValue<T>(in string key, out T o);
        object this[string key] { get; set; }
        void Set<T>(in string key, T value);
        void Remove(in string key);
        bool ContainsKey(in string key);

        void Clear();
    }

    public class StateMachine<TOwner> : IStateMachine<TOwner>
    {
        public event Action<IState<TOwner>, IState<TOwner>> OnStateChange = delegate { };
        public bool running { get; protected set; }
        public TOwner owner { get; protected set; }
        public IState<TOwner> currentState { get; protected set; }
        private readonly IBlackboard _blackboard;
        private readonly Dictionary<int, IState<TOwner>> _states;

        public IEnumerable<IState<TOwner>> States => _states.Values;

        public StateMachine(TOwner owner, IBlackboard blackboard)
        {
            this.owner = owner;
            _blackboard = blackboard;
            _states = new Dictionary<int, IState<TOwner>>(8);
            currentState = default;
        }

        public StateMachine(TOwner owner)
        {
            this.owner = owner;
            _blackboard = new Blackboard();
            _states = new Dictionary<int, IState<TOwner>>(8);
            currentState = default;
        }

        public void SetParam<T>(string key, T value)
        {
            _blackboard[key] = value;
        }

        public T GetParam<T>(string key)
        {
            if (_blackboard.TryGetValue(key, out var value))
            {
                return (T)value;
            }

            return default;
        }

        public bool Change<T>() where T : IState<TOwner>
        {
            var typeId = TypeId<T>.stableId;
            if (_states.TryGetValue(typeId, out var state))
            {
                OnStateChange(currentState, state);
                currentState.OnExit(owner, this);
                currentState = state;
                currentState.OnEnter(owner, this);
                return true;
            }

            ToolkitLog.Error("StateMachine Change State Failed: State Not Found Type=" + typeof(T).FullName);
            return false;
        }

        public bool Change(Type type)
        {
            if (_states.TryGetValue(TypeId.StableId(type), out var state))
            {
                OnStateChange(currentState, state);
                currentState.OnExit(owner, this);
                currentState = state;
                currentState.OnEnter(owner, this);
                return true;
            }

            return false;
        }

        public void Add<T>() where T : IState<TOwner>, new()
        {
            Add(new T());
        }

        public void Add<T>(T state) where T : IState<TOwner>
        {
            _states.Add(TypeId<T>.stableId, state);
            state.OnInit(owner, this);
        }

        public void Run<T>() where T : IState<TOwner>
        {
            currentState = _states[TypeId<T>.stableId];
            currentState.OnEnter(owner, this);
            OnStateChange(null, currentState);
            running = true;
        }

        public void Run(Type type)
        {
            currentState = _states[TypeId.StableId(type)];
            currentState.OnEnter(owner, this);
            OnStateChange(null, currentState);
            running = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnUpdate()
        {
            if (!running) return;
            currentState.Transition(owner, this);
            currentState.OnUpdate(owner, this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void RemoveParam(string key)
        {
            _blackboard.Remove(key);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsParam(string battleSettlementData)
        {
            return _blackboard.ContainsKey(battleSettlementData);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop()
        {
            currentState.OnExit(owner, this);
            currentState = default;

            foreach (var statesValue in _states.Values)
            {
                statesValue.OnDispose(owner, this);
            }

            running = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetState<T>() where T : IState<TOwner>
        {
            return (T)_states[TypeId<T>.stableId];
        }
    }


    public interface ITaskState<TOwner> : IState
    {
        Task OnInit(TOwner owner, IStateMachine<TOwner> stateMachine);
        Task OnDispose(TOwner owner, IStateMachine<TOwner> stateMachine);
        Task OnEnter(TOwner owner, IStateMachine<TOwner> stateMachine);
        Task Transition(TOwner owner, IStateMachine<TOwner> stateMachine);
        Task OnUpdate(TOwner owner, IStateMachine<TOwner> stateMachine);
        Task OnExit(TOwner owner, IStateMachine<TOwner> stateMachine);
    }

    public interface ITaskStateMachine<TOwner>
    {
        event Action<ITaskState<TOwner>, ITaskState<TOwner>> OnStateChange;
        bool running { get; }
        TOwner owner { get; }
        ITaskState<TOwner> currentState { get; }
        void SetParam<T>(string key, T value);
        Task<T> GetParam<T>(string key);
        Task<bool> Change<T>() where T : ITaskState<TOwner>;
        Task<bool> Change(Type type);
        Task Run<T>() where T : ITaskState<TOwner>;
        Task Run(Type type);
        Task Add<T>(T state) where T : ITaskState<TOwner>;
        Task Stop();
        Task OnUpdate();
        bool RemoveParam(string key);
        bool ContainsParam(string battleSettlementData);
    }


    // public class TaskStateMachine<TOwner> : ITaskStateMachine<TOwner>
    // {
    // }
}