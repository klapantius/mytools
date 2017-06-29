using System;
using System.IO;


namespace rsfainstanalyzer
{
    public interface ILogIterator
    {
        void Process(string path, string logname, int days, Action<TextReader, string> fileLevelAction, Action<string> directoryLevelAction);
    }
}