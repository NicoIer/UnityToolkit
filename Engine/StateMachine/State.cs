namespace UnityToolkit
{
    public interface IState
    {
    }

    public interface IState<in T> : IState
    {
        public void OnEnter(T owner);

        public void OnUpdate(T owner);

        public void OnExit(T owner);
    }

    public interface IStateMachine<TOwner>
    {
        public void Setup(TOwner owner);
        public bool Change<T>(TOwner owner) where T : IState<TOwner>;
        public void Add<T>(T state) where T : IState<TOwner>;
        public void Add<T>() where T : IState<TOwner>, new();
        public void OnUpdate(TOwner owner);
    }
}