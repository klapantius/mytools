using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Schema;

using juba.consoleapp;
using juba.consoleapp.CmdLine;
using juba.consoleapp.Out;

using sybi;
using sybi.RSFA;

using SimpleInjector;


namespace rsfainstanalyzer
{
    public class Program
    {
        static void Main(string[] args)
        {
            var ioc = new Container();
            ioc.Register<ICmdLineInterpreter, Interpreter>(Lifestyle.Singleton);
            ioc.Register<IStepTimeAnalyzer, StepTimeAnalyzer>(Lifestyle.Singleton);
            ioc.Register<ILogIterator, LogIterator>();
            ioc.Register<IBuildNameExtractor, BuildNameExtractor>();
            ioc.Register<IConsoleAppOut, ConsoleOut>(Lifestyle.Singleton);

            var myOut = ioc.GetInstance<IConsoleAppOut>();
            var analyzer = ioc.GetInstance<IStepTimeAnalyzer>();
            analyzer.ThrowExceptionOnError = true;
            var cmd = ioc.GetInstance<ICmdLineInterpreter>();
            cmd.Add(new Parameter(new[] { "path" }, "path (wildcards are enabled at the end)", "path to the rsfa install log", true, @"\\fors34ba.ww005.siemens.net\tfssysint$\"));
            cmd.Add(new Parameter(new[] { "days" }, "count", "number of days in the past from now - another way to filter", false, "0"));
            cmd.Add(new Command(new[] { "top" }, "finds longest executing steps in each log files", () =>
            {
                var days = cmd.Evaluate("days", Interpreter.DefaultIntConverter);
                ioc.GetInstance<ILogIterator>().Process(cmd.ValueOf("path"), cmd.ValueOf("logname"), days,
                    (input, folder) =>
                    {
                        var steps = analyzer.FindLongestSteps(input);
                        steps.ForEach(s => myOut.Info("\t{0}: {1}", s.Duration, s.Step));
                    }, null);
            }));

            //cmd.Add(new Command(new[] { "sortbytime" }, "finds longest steps of ALL matching logs", () =>
            //{
            //    myOut.Error("This is not available yet.");
            //    //var results = new List<StepTimeAnalyzer.Result>();
            //    //results.AddRange(steps);
            //}));

            cmd.Add(new Command(new[] { "scriptduration", "sd" }, "calculates the average script execution duration", () =>
            {
                var results = new List<ScriptDurationData>();
                var dirdur = new List<ScriptDurationData>();
                var days = cmd.Evaluate("days", Interpreter.DefaultIntConverter);
                var jsonout = cmd.Evaluate("json", Interpreter.DefaultBoolConverter);
                ioc.GetInstance<ILogIterator>().Process(cmd.ValueOf("path"), cmd.ValueOf("logname"), days,
                    (log, folder) =>
                    {
                        var d = ioc.GetInstance<ScriptDurationData>();
                        try
                        {
                            d.Duration = analyzer.GetScriptDuration(log);
                            d.DropFolder = folder;
                            results.Add(d);
                            dirdur.Add(d);
                        }
                        catch (RSFADurationCalculationException)
                        {
                        }
                    },
                    (dirname) =>
                    {
                        if (!dirdur.Any()) return;
                        myOut.Info("Average duration in {0}: {1}", dirname.Substring(Path.GetDirectoryName(dirname).Length), TimeSpan.FromSeconds(dirdur.Average(d => d.Duration.TotalSeconds)));
                        dirdur.Clear();
                    });
                if (results.Any())
                {
                    var averageDuration = TimeSpan.FromSeconds(results.Average(d => d.Duration.TotalSeconds));
                    var max = results.Max();
                    var min = results.Min();
                    if (!jsonout)
                    {
                        myOut.Info("Raw average for {0} ({1} executions): {2}", cmd.ValueOf("path"), results.Count, averageDuration.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture));
                        myOut.Info("Longest RSFA install: {0} in {1} ( {2} )", max.Duration, max.BuildName, max.DropFolder);
                        myOut.Info("Fastest RSFA install: {0} in {1} ( {2} )", min.Duration, min.BuildName, min.DropFolder);
                    }
                    else
                    {
                        myOut.Info("{{\"avgtxt\": \"{0}\", \"max\": \"{1}\", \"maxlink\": \"{2}\", \"min\": \"{3}\", \"minlink\": \"{4}\", \"avgraw\": {5}}}", averageDuration.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture), max.Duration, "dropfolder1", min.Duration, "drop2", Math.Round(averageDuration.TotalSeconds));
                    }
                }
                else {
                    Console.WriteLine("no results at all");
                }
            }));
            cmd.Add(new Parameter(new[] { "json", "jsonout" }, "option", "machine output for further processing", false, "false")).BelongsTo("scriptduration");

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
            myOut.VerbosityLevel = cmd.Evaluate("verbose", Interpreter.DefaultBoolConverter) ? 1 : 0;

            cmd.ExecuteCommands();

            if (cmd.Evaluate("anykey", Interpreter.DefaultBoolConverter))
            {
                Console.WriteLine("\n\ndone, please press a key to finish the execution");
                Console.ReadKey();
            }
        }

    }
}
