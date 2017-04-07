using System.Collections.Generic;


namespace juba.consoleapp.CmdLine
{
    public interface ICmdLineCommand : ICmdLineItem
    {
        void Requires(params string[] items);
        List<string> RequiredParams { get; }
        void Execute();
    }
}