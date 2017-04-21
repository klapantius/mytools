using System;
using System.Collections.Generic;
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
            ioc.Register<ILogIterator, LogIterator>();

            var analyzer = ioc.GetInstance<StepTimeAnalyzer>();
            var cmd = ioc.GetInstance<ICmdLineInterpreter>();
            cmd.Add(new Parameter(new[] { "path" }, "path (wildcards are enabled at the end)", "path to the rsfa install log", true, @"\\fors34ba.ww005.siemens.net\tfssysint$\"));
            cmd.Add(new Command(new[] { "groupbyfile" }, "finds longest executing steps in each log files", () =>
            {
                ioc.GetInstance<ILogIterator>().Process(cmd.ValueOf("path"), cmd.ValueOf("logname"), (input) =>
                {
                    var steps = analyzer.FindLongestSteps(input);
                    steps.ForEach(s => Out.Info("\t{0}: {1}", s.Duration, s.Step));
                });
            }));

            cmd.Add(new Command(new[] { "sortbytime" }, "finds longest steps of ALL matching logs", () =>
            {
                Out.Error("This is not available yet.");
                //var results = new List<StepTimeAnalyzer.Result>();
                //results.AddRange(steps);
            }));

            cmd.Add(new Command(new[] { "scriptduration", "sd" }, "calculates the average script execution duration", () =>
            {
                var durations = new List<TimeSpan>();
                ioc.GetInstance<ILogIterator>().Process(cmd.ValueOf("path"), cmd.ValueOf("logname"), (input) => 
                    durations.Add(analyzer.GetScriptDuration(input)));
                var averageDuration = TimeSpan.FromSeconds(durations.Average(d => d.TotalSeconds));
                Out.Info("Raw average for {0} ({1} executions): {2}", cmd.ValueOf("path"), durations.Count, averageDuration);
            }));

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
