namespace Nico
{
    public abstract class State<T>
    {
        protected T owner;

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