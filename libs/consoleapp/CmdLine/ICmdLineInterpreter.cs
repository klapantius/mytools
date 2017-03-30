using System;

namespace juba.consoleapp.CmdLine
{
    public interface ICmdLineInterpreter
    {
        string ValueOf(string paramName);
        string ValueOf(string paramName, Func<bool, string> validation);
        T Evaluate<T>(string paramName, Func<string, T> converter, params Action<T>[] validator);
        bool IsSpecified(string paramName);
        Parameter Add(Parameter parameter);
        bool Parse(params string[] inArgs);
        void PrintErrors(string prgName);
    }
}