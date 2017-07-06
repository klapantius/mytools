using System;

using Microsoft.TeamFoundation.Build.Client;
using juba.consoleapp;
using juba.tfs.interfaces;
using juba.tfs.wrappers;

using SimpleInjector;

using juba.consoleapp.CmdLine;
using juba.consoleapp.Out;

using Command = juba.consoleapp.CmdLine.Command;


namespace testrunanalyzer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var ioc = new Container();
            ioc.Register<ICmdLineInterpreter, Interpreter>(Lifestyle.Singleton);
            ioc.Register<IConsoleAppOut, ConsoleOut>(Lifestyle.Singleton);
            ioc.Register(() => new Uri(ioc.GetInstance<ICmdLineInterpreter>().ValueOf("tpc")), Lifestyle.Singleton);
            ioc.Register<ITfsTeamProjectCollection, TfsTeamProjectCollectionWrapper>(Lifestyle.Singleton);
            ioc.Register(() => ioc.GetInstance<ITfsTeamProjectCollection>().GetService<IBuildServer>());
            ioc.Register<ITestManagementTeamProjectProvider, TestManagementTeamProjectProvider>();
            ioc.Register(() => ioc.GetInstance<ITestManagementTeamProjectProvider>().GeTestManagementTeamProject(ioc.GetInstance<ICmdLineInterpreter>().ValueOf("tp")));
            ioc.Register<ITestExecutionDataCollector, TestExecutionDataCollector>();
            ioc.Register<TestExecutionAnalyzer>();
            ioc.Register<AssemblyAnalyzer>();

            var output = ioc.GetInstance<IConsoleAppOut>();
            var cmd = ioc.GetInstance<ICmdLineInterpreter>();
            cmd.Add(new Parameter(new[] { "teamproject", "tp" }, "name", "team project name", false, "syngo.net"));
            cmd.Add(new Parameter(new[] { "teamprojectcollection", "tpc" }, "uri", "team project collection uri", false, "https://tfs.healthcare.siemens.com:8090/tfs/ikm.tpc.projects"));
            cmd.Add(new Parameter(new[] { "verbose", "v" }, "bool", "verbose mode - lists all builds and executions while collecting them", false, "false"));
            cmd.Add(new Parameter(new[] { "anykey" }, "bool", "doesn't exit at the end", false, "false"));

            cmd.Add(new Parameter(new[] { "build" }, "name or id", "build id or build definition name (wildchars accepted)", false));
            cmd.Add(new Parameter(new[] { "days" }, "count", "number of asked days if build definition is specified", false, "1")).BelongsTo("top10", "findassembly");
            cmd.Add(new Parameter(new[] { "threshold" }, "minutes", "longest accepted testrun duration in minutes", false, "20")).BelongsTo("top10");
            cmd.Add(new Parameter(new[] { "peakfilter" }, "percent", "% of runs longer then threshold to take it as relevant", false, "33")).BelongsTo("top10");
            cmd.Add(new Parameter(new[] { "extendedoutput", "ext" }, "bool", "display the durations taken", false, "false")).BelongsTo("top10");
            cmd.Add(new Command(new[] { "top10" }, "list the 10 longest running assemblies", () =>
            {
                var a = ioc.GetInstance<TestExecutionAnalyzer>();
                a.Analyze(
                    cmd.ValueOf("tp"),
                    cmd.ValueOf("build"),
                    cmd.Evaluate("days", Interpreter.DefaultIntConverter,
                        (x) => { if (x <= 0) throw new ExceptionBase("The specified number of days is not valid."); }),
                    cmd.Evaluate("threshold", Interpreter.DefaultIntConverter,
                        (x) => { if (x <= 0) throw new ExceptionBase("The specified duration threshold is not valid."); }),
                    cmd.Evaluate("peakfilter", Interpreter.DefaultIntConverter,
                        (x) => { if (x <= 0 || x > 100) throw new ExceptionBase("The specified peak filter is not valid. It must be in range 1-100"); }),
                    cmd.Evaluate("extendedoutput", Interpreter.DefaultBoolConverter)
                    );

            })).Requires("build");
            cmd.Add(new Parameter(new[] {"assembly"}, "name or regex", "specification of asked assembly", false)).BelongsTo("findassembly");
            cmd.Add(new Command(new []{"teststatistic", "stat"}, "collect the executions of a specified assembly", () =>
            {
                var a = ioc.GetInstance<AssemblyAnalyzer>();
                a.Analyze(
                    cmd.ValueOf("tp"),
                    cmd.ValueOf("build"),
                    cmd.Evaluate("days", Interpreter.DefaultIntConverter,
                        (x) => { if (x <= 0) throw new ExceptionBase("The specified number of days is not valid."); }),
                    cmd.ValueOf("assembly")
                    );
                
            })).Requires("build", "assembly");
            cmd.Add(new Command(new []{"usage"}, "further help", () =>
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("\t- Find long runners of a module. Example: testrunanalyzer /top10 /build:modules.foundations.main.*gc /days:21");
                Console.WriteLine("\t- Find the longest runners at all. Example: testrunanalyzer /top10 /build:modules.*.main.*gc /days:30 /extendedoutput");
                Console.WriteLine("\t  The above one displays the durations taken in count too.");
                Console.WriteLine("\t- Display the execution durations of a test. Example: testrunanalyzer /findassembly /assembly:syngo.BizLogic.Modules.Viewing.BasicImaging.Tools_iTest /build:modules.itf.main.*gc /days:21");
                Console.WriteLine("\t  The above one is useful to see the trending if any.");
            }));

            if (!cmd.Parse(args))
            {
                cmd.PrintErrors("testrunanalyzer.exe");
                if (cmd.Evaluate("anykey", Interpreter.DefaultBoolConverter))
                {
                    Console.WriteLine("\n\ndone, please press a key to finish the execution");
                    Console.ReadKey();
                }
                return;
            }
            output.VerbosityLevel = cmd.Evaluate("verbose", Interpreter.DefaultBoolConverter) ? 1 : 0;

            cmd.ExecuteCommands();

            if (cmd.Evaluate("anykey", Interpreter.DefaultBoolConverter))
            {
                Console.WriteLine("\n\ndone, please press a key to finish the execution");
                Console.ReadKey();
            }
        }
    }
}
