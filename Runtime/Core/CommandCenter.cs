namespace UnityToolkit
{
    public interface ICommand
    {
        void Execute();
    }
    public interface UndoableCommand : ICommand
    {
        void Undo();
    }
}