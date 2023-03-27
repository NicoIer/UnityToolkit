namespace Nico.Design
{
    /// <summary>
    /// 状态
    /// </summary>
    /// <typeparam name="T">状态持有者</typeparam>
    public interface IState<T>where T : class
    {
        public void Enter();
        public void Exit();
        public void Update();
    }
}