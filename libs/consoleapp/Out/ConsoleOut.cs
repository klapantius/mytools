using System;

namespace juba.consoleapp.Out
{
    public class ConsoleOut : ConsoleAppDefaultOut, IConsoleAppOut
    {
        public override int VerbosityLevel
        {
            get { return myVerbosityLevel; }
            set { if (value >= 0) myVerbosityLevel = value; }
        }

        public new void Log(string fmt, params object[] args) { if (VerbosityLevel > 0) Print(ConsoleColor.DarkGray, fmt, args); }
        public new void Info(string fmt, params object[] args) { Print(fmt, args); }
        public new void Error(string fmt, params object[] args) { Print(ConsoleColor.Red, fmt, args); }

        private static void Print(ConsoleColor color, string fmt, params object[] args)
        {
            Console.ForegroundColor = color;
            Print(fmt, args);
            Console.ResetColor();
        }
        private static void Print(string fmt, params object[] args)
        {
            Console.WriteLine(fmt, args);
        }
    }
}
