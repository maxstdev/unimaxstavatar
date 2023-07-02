namespace Maxst.Avatar
{
    public interface ICommand
    {
        void Execute();
        void Undo();
    }
}