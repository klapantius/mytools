using System;


namespace juba.consoleapp
{
  public static class Out
  {
    public static int VerbosityLevel = 0;
    public static void Log(string fmt, params object[] args) { if (VerbosityLevel > 0) Print(ConsoleColor.DarkGray, fmt, args); }
    public static void Info(string fmt, params object[] args) { Print(fmt, args); }
    public static void Error(string fmt, params object[] args) { Print(ConsoleColor.Red, fmt, args); }

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