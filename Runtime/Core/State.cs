using System.Collections.Generic;

namespace UnityToolkit
{
    public interface IState
    {
    }

    public interface IState<T> : IState
    {
        public void OnInit(T owner, IStateMachine<T> stateMachine);
        public void OnEnter(T owner, IStateMachine<T> stateMachine);
        public void OnUpdate(T owner, IStateMachine<T> stateMachine);
        public void OnExit(T owner, IStateMachine<T> stateMachine);
    }

    public interface IStateMachine<TOwner>
    {
        public bool running { get; }
        public TOwner owner { get; }
        public IState<TOwner> currentState { get; }
        public void SetParm<T>(string key, T value);
        public T GetParam<T>(string key);
        public bool Change<T>() where T : IState<TOwner>;
        public void Run<T>() where T : IState<TOwner>;
        public void Add<T>(T state) where T : IState<TOwner>;
        public void Add<T>() where T : IState<TOwner>, new() => Add(new T());
        public void OnUpdate();
    }

    public class StateMachine<TOwner> : IStateMachine<TOwner>
    {
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

        public void SetParm<T>(string key, T value)
        {
            _blackboard[key] = value;
        }

        public T GetParam<T>(string key)
        {
            return (T)_blackboard[key];
        }

        public bool Change<T>() where T : IState<TOwner>
        {
            var typeId = TypeId<T>.stableId;
            if (_states.TryGetValue(typeId, out var state))
            {
                currentState.OnExit(owner, this);
                currentState = state;
                currentState.OnEnter(owner, this);
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
            running = true;
        }

        public void OnUpdate()
        {
            if (!running) return;
            currentState.OnUpdate(owner, this);
        }
    }
}