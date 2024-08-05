using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityToolkit
{
    public interface IState
    {
    }

    public interface IState<T> : IState
    {
        void OnInit(T owner, IStateMachine<T> stateMachine);
        void OnEnter(T owner, IStateMachine<T> stateMachine);
        void OnUpdate(T owner, IStateMachine<T> stateMachine);
        void OnExit(T owner, IStateMachine<T> stateMachine);
    }

    public interface IStateMachine<TOwner>
    {
        event Action<Type, Type> OnStateChange;
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

    public class StateMachine<TOwner> : IStateMachine<TOwner>
    {
        public event Action<Type, Type> OnStateChange = delegate { };
        public bool running { get; protected set; }
        public TOwner owner { get; protected set; }
        public IState<TOwner> currentState { get; protected set; }
        private readonly Dictionary<string, object> _blackboard;
        private readonly Dictionary<int, IState<TOwner>> _states;

        public StateMachine(TOwner owner)
        {
            this.owner = owner;
            _blackboard = new Dictionary<string, object>();
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
                currentState.OnExit(owner, this);
                currentState = state;
                currentState.OnEnter(owner, this);
                OnStateChange(currentState.GetType(), typeof(T));
                return true;
            }

            return false;
        }

        public bool Change(Type type)
        {
            if (_states.TryGetValue(TypeId.StableId(type), out var state))
            {
                currentState.OnExit(owner, this);
                currentState = state;
                currentState.OnEnter(owner, this);
                OnStateChange(currentState.GetType(), type);
                return true;
            }

            return false;
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
            OnStateChange(null, typeof(T));
            running = true;
        }

        public void Run(Type type)
        {
            currentState = _states[TypeId.StableId(type)];
            currentState.OnEnter(owner, this);
            OnStateChange(null, type);
            running = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void OnUpdate()
        {
            if (!running) return;
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
            running = false;
        }
    }
}