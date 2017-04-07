using System.Collections.Generic;


namespace juba.consoleapp.CmdLine
{
    public interface ICmdLineItem
    {
        string[] Names { get; }
        string Description { get; }
        bool Matches(ICmdLineItem other);
        bool Matches(string name);
        string Help(bool verbose);
    }
}