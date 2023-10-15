using System;

namespace UnityToolkit
{
    public interface ICommand
    {
        void Execute();
    }

    public struct CustomUnRegister : ICommand
    {
        private Action _unRegister;

        public CustomUnRegister(Action action)
        {
            _unRegister = action;
        }

        public void Execute()
        {
            _unRegister.Invoke();
            _unRegister = null;
        }
    }
    
    public interface UndoableCommand : ICommand
    {
        void Undo();
    }
    // public sealed class CommandCenter
    // {
    //     
    // }
}