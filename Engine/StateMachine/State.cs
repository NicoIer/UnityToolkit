namespace UnityToolkit
{
    public abstract class State<T>
    {
        public virtual void OnEnter(T owner)
        {
        }

        public virtual void OnUpdate(T owner)
        {
        }

        public virtual void OnExit(T owner)
        {
        }
    }
}