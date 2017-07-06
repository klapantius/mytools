using System;

namespace juba.consoleapp.Out
{
    public class TextFileOut : IConsoleAppOut
    {
        public int VerbosityLevel
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Log(string fmt, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Info(string fmt, params object[] args)
        {
            throw new NotImplementedException();
        }

        public void Error(string fmt, params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}
