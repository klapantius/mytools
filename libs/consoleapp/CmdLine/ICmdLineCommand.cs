namespace juba.consoleapp.CmdLine
{
    public interface ICmdLineCommand : ICmdLineItem
    {
        void Execute();
    }
}