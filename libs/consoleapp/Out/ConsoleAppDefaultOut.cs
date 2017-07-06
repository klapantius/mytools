using System;

namespace juba.consoleapp.Out
{
    public class ConsoleAppDefaultOut : IConsoleAppOut
    {
        protected int myVerbosityLevel;

        public virtual int VerbosityLevel
        {
            get { return myVerbosityLevel; }
            set { if (value >= 0) myVerbosityLevel = value; }
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
