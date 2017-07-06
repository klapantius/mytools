namespace juba.consoleapp.Out
{
    public interface IConsoleAppOut
    {
        int VerbosityLevel { get; set; }
        void Log(string fmt, params object[] args);
        void Info(string fmt, params object[] args);
        void Error(string fmt, params object[] args);
    }
}