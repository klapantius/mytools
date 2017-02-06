using System;


namespace consoleapp
{
  public static class Out
  {
    public static int VerbosityLevel = 0;
    public static void Log(string fmt, params object[] args) { if (VerbosityLevel > 0) Print(ConsoleColor.DarkGray, fmt, args); }

    private static void Print(ConsoleColor color, string fmt, params object[] args)
    {
      Console.ForegroundColor = color;
      Console.WriteLine(fmt, args);
      Console.ResetColor();
    }
  }
}