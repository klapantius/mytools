using System;

using Microsoft.TeamFoundation.Build.Client;
using juba.consoleapp;
using juba.tfs.interfaces;
using juba.tfs.wrappers;

using SimpleInjector;

using juba.consoleapp.CmdLine;
using Command=juba.consoleapp.CmdLine.Command;


namespace testrunanalyzer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var ioc = new Container();
            ioc.Register<ICmdLineInterpreter, Interpreter>(Lifestyle.Singleton);
            ioc.Register(() => new Uri(ioc.GetInstance<ICmdLineInterpreter>().ValueOf("tpc")), Lifestyle.Singleton);
            ioc.Register<ITfsTeamProjectCollection, TfsTeamProjectCollectionWrapper>(Lifestyle.Singleton);
            ioc.Register(() => ioc.GetInstance<ITfsTeamProjectCollection>().GetService<IBuildServer>());
            ioc.Register<ITestManagementTeamProjectProvider, TestManagementTeamProjectProvider>();
            ioc.Register(() => ioc.GetInstance<ITestManagementTeamProjectProvider>().GeTestManagementTeamProject(ioc.GetInstance<ICmdLineInterpreter>().ValueOf("tp")));
            ioc.Register<ITestExecutionDataCollector, TestExecutionDataCollector>();
            ioc.Register<TestExecutionAnalyzer>();

            var cmd = ioc.GetInstance<ICmdLineInterpreter>();
            cmd.Add(new Parameter(new[] { "build" }, "build id or build definition name (wildchars accepted)", false));
            cmd.Add(new Parameter(new[] { "days" }, "number of asked days if build definition is specified", false, "1"));
            cmd.Add(new Parameter(new[] { "teamproject", "tp" }, "team project name", false, "syngo.net"));
            cmd.Add(new Parameter(new[] { "teamprojectcollection", "tpc" }, "team project collection uri", false, "https://tfs.healthcare.siemens.com:8090/tfs/ikm.tpc.projects"));
            cmd.Add(new Parameter(new[] { "verbose", "v", "debug", "d" }, "verbose mode", false, "false"));
            cmd.Add(new Command(new[] { "top10assemblies" }, "lists the 10 longest assemblies", () =>
            {
                var a = ioc.GetInstance<TestExecutionAnalyzer>();
                a.Analyze(
                    cmd.ValueOf("tp"),
                    cmd.ValueOf("build"),
                    cmd.Evaluate("days", Interpreter.DefaultIntConverter,
                        (x) => { if (x <= 0) throw new Exception("The specified number of days is not valid"); }));

            })).Requires("build");
            cmd.Add(new Parameter(new[] {"Iam"}, "a name", false));
            cmd.Add(new Command(new []{"hello"}, "greeting", () => Out.Info("hello {0}", cmd.ValueOf("iam")))).Requires("iam");

            if (!cmd.Parse(args))
            {
                cmd.PrintErrors("testrunanalyzer.exe");
                Console.ReadKey();
                return;
            }
            Out.VerbosityLevel = cmd.Evaluate("verbose", Interpreter.DefaultBoolConverter) ? 1 : 0;

            cmd.ExecuteCommands();

            Console.WriteLine("\n\ndone");
            Console.ReadKey();
        }
    }
}
