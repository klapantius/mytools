using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;

using juba.consoleapp;
using juba.consoleapp.CmdLine;

using sybi;

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
            ioc.Register<IBuildNameExtractor, BuildNameExtractor>();

            var analyzer = ioc.GetInstance<StepTimeAnalyzer>();
            var cmd = ioc.GetInstance<ICmdLineInterpreter>();
            cmd.Add(new Parameter(new[] { "path" }, "path (wildcards are enabled at the end)", "path to the rsfa install log", true, @"\\fors34ba.ww005.siemens.net\tfssysint$\"));
            cmd.Add(new Parameter(new[] { "days" }, "count", "number of days in the past from now - another way to filter", false, "0"));
            cmd.Add(new Command(new[] { "top", "groupbyfile" }, "finds longest executing steps in each log files", () =>
            {
                var days = cmd.Evaluate("days", Interpreter.DefaultIntConverter);
                ioc.GetInstance<ILogIterator>().Process(cmd.ValueOf("path"), cmd.ValueOf("logname"), days,
                    (input, folder) =>
                    {
                        var steps = analyzer.FindLongestSteps(input);
                        steps.ForEach(s => Out.Info("\t{0}: {1}", s.Duration, s.Step));
                    }, null);
            }));

            cmd.Add(new Command(new[] { "sortbytime" }, "finds longest steps of ALL matching logs", () =>
            {
                Out.Error("This is not available yet.");
                //var results = new List<StepTimeAnalyzer.Result>();
                //results.AddRange(steps);
            }));

            cmd.Add(new Command(new[] { "scriptduration", "sd" }, "calculates the average script execution duration", () =>
            {
                var results = new List<ScriptDurationData>();
                var dirdur = new List<ScriptDurationData>();
                var days = cmd.Evaluate("days", Interpreter.DefaultIntConverter);
                ioc.GetInstance<ILogIterator>().Process(cmd.ValueOf("path"), cmd.ValueOf("logname"), days,
                    (log, folder) =>
                    {
                        var d = ioc.GetInstance<ScriptDurationData>();
                        d.Duration = analyzer.GetScriptDuration(log);
                        d.DropFolder = folder;
                        results.Add(d);
                        dirdur.Add(d);
                    },
                    (dirname) =>
                    {
                        if (!dirdur.Any()) return;
                        Out.Info("Average duration in {0}: {1}", dirname.Substring(Path.GetDirectoryName(dirname).Length), TimeSpan.FromSeconds(dirdur.Average(d => d.Duration.TotalSeconds)));
                        dirdur.Clear();
                    });
                var averageDuration = TimeSpan.FromSeconds(results.Average(d => d.Duration.TotalSeconds));
                Out.Info("Raw average for {0} ({1} executions): {2}", cmd.ValueOf("path"), results.Count, averageDuration);
                var max = results.Max();
                var min = results.Min();
                Out.Info("Longest RSFA install: {0} in {1} ( {2} )", max.Duration, max.BuildName, max.DropFolder);
                Out.Info("Fastest RSFA install: {0} in {1} ( {2} )", min.Duration, min.BuildName, min.DropFolder);
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
