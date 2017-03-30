using System.Collections.Generic;

namespace juba.consoleapp.CmdLine
{
    public interface ICmdLineParameter
    {
        string[] Names { get; }
        string Description { get; }
        bool IsMandatory { get; }
        string DefaultValue { get; }
        string Value { get; }
        bool Matches(Parameter other);
        bool Matches(string name);
        string ToString();
        List<string> CoParams { get; }
        void Requires(params string[] coparams);
    }
}