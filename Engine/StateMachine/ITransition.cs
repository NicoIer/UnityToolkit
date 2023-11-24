using System;

namespace UnityToolkit
{
    public interface ITransition<TOwner>
    {
        public bool GetNext(out Type type, StateMachine<TOwner> stateMachine, TOwner owner);
    }
}