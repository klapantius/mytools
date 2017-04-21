using System;
using System.IO;
using System.Linq;

using juba.consoleapp;
using juba.consoleapp.CmdLine;

using SimpleInjector;


namespace rsfainstanalyzer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var ioc = new Container();
            ioc.Register<ICmdLineInterpreter, Interpreter>(Lifestyle.Singleton);
            ioc.Register<StepTimeAnalyzer, StepTimeAnalyzer>();

            var cmd = ioc.GetInstance<ICmdLineInterpreter>();
            cmd.Add(new Parameter(new[] { "path" }, "path (wildcards are enabled at the end)", "path to the rsfa install log", false, @"\\fors34ba.ww005.siemens.net\tfssysint$\"));
            cmd.Add(new Command(new[] { "byfile" }, "find longest executing steps in the install script", () =>
            {
                var analyzer = ioc.GetInstance<StepTimeAnalyzer>();
                var root = Path.GetDirectoryName(cmd.ValueOf("path"));
                var dirPattern = cmd.ValueOf("path").Substring(root.Length + 1);
                var dnames = Directory.GetFileSystemEntries(root, dirPattern, SearchOption.TopDirectoryOnly);
                dnames.ToList().ForEach(d =>
                {
                    var fnames = Directory.GetFileSystemEntries(d, cmd.ValueOf("logname"), SearchOption.AllDirectories);
                    fnames.ToList().ForEach(f =>
                    {
                        try
                        {
                            using (var input = new StreamReader(f))
                            {
                                Out.Info(f);
                                var steps = analyzer.FindLongestSteps(input);
                                steps.ForEach(s => Out.Info("\t{0}: {1}", s.Duration, s.Step));
                            }
                        }
                        catch (IOException) { }
                    });
                });
            })).Requires("path");

            cmd.Add(new Parameter(new[] { "logname" }, "pattern", "name pattern of an rsfa install log file", false, "rsfa.install.*.log"));
            cmd.Add(new Parameter(new[] { "verbose", "v" }, "bool", "verbose mode - lists all builds and executions while collecting them", false, "false"));
            cmd.Add(new Parameter(new[] { "anykey" }, "bool", "doesn't exit at the end", false, "false"));

            if (!cmd.Parse(args))
            {
                cmd.PrintErrors("rsfainstanalyzer.exe");
                if (cmd.Evaluate("anykey", Interpreter.DefaultBoolConverter))
                {
                    Console.WriteLine("\n\ndone, please press a key to finish the execution");
                    Console.ReadKey();
                }
                return;
            }
            Out.VerbosityLevel = cmd.Evaluate("verbose", Interpreter.DefaultBoolConverter) ? 1 : 0;

            cmd.ExecuteCommands();

            if (cmd.Evaluate("anykey", Interpreter.DefaultBoolConverter))
            {
                Console.WriteLine("\n\ndone, please press a key to finish the execution");
                Console.ReadKey();
            }
        }
    }
}
