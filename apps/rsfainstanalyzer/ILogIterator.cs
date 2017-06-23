using System;
using System.IO;


namespace rsfainstanalyzer
{
    public interface ILogIterator
    {
        void Process(string path, string logname, Action<TextReader> fileLevelAction, Action<string> directoryLevelAction);
    }
}