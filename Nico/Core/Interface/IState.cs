namespace Nico.Design
{
    /// <summary>
    /// 状态
    /// </summary>
    /// <typeparam name="T">状态持有者</typeparam>
    public interface IState<T>where T : class
    {
        public T Owner { get;  }
        public IStateMachine<T> StateMachine { get;  }
        public void Init(T owner, IStateMachine<T> stateMachine);
        public void Enter();
        public void Exit();
        public void Update();
    }
}