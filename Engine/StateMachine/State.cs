namespace UnityToolkit
{
    public abstract class State<T>
    {
        public T owner { get; private set; }
        public State(T owner)
        {
            this.owner = owner;
        }
        public virtual void OnEnter()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnExit()
        {
        }
    }
}