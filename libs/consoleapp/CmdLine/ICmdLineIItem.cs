using System.Collections.Generic;


namespace juba.consoleapp.CmdLine
{
    public interface ICmdLineItem
    {
        string[] Names { get; }
        string Description { get; }
        List<string> RequiredParams { get; }
        void Requires(params string[] items);
        bool Matches(ICmdLineItem other);
        bool Matches(string name);
        string Help(bool verbose);
    }
}