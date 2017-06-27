using System;
using System.IO;


namespace rsfainstanalyzer
{
    public interface ILogIterator
    {
        void Process(string path, string logname, Action<TextReader, string> fileLevelAction, Action<string> directoryLevelAction);
    }
}