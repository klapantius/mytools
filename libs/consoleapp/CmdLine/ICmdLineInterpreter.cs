using System;

namespace juba.consoleapp.CmdLine
{
    public interface ICmdLineInterpreter
    {
        string ValueOf(string paramName);
        string ValueOf(string paramName, Func<bool, string> validation);
        T Evaluate<T>(string paramName, Func<string, T> converter, params Action<T>[] validator);
        bool IsSpecified(string paramName);
        ICmdLineParameter Add(ICmdLineParameter parameter);
        ICmdLineCommand Add(ICmdLineCommand command, params string[] requires);
        bool Parse(params string[] inArgs);
        void ExecuteCommands();
        void PrintErrors(string prgName);
        void PrintUsage(string prgName);
    }
}