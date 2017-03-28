using System;

using Microsoft.TeamFoundation.Build.Client;
using juba.consoleapp;
using juba.tfs.interfaces;
using juba.tfs.wrappers;

using Microsoft.TeamFoundation.TestManagement.Client;

using SimpleInjector;

using CmdLine = juba.consoleapp.CmdLine;


namespace testrunanalyzer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var cmd = new CmdLine.Interpreter();
            cmd.Add(new CmdLine.Parameter(new[] { "build" }, "build definition name", false));
            cmd.Add(new CmdLine.Parameter(new[] { "teamproject", "tp" }, "team project name", false, "syngo.net"));
            cmd.Add(new CmdLine.Parameter(new[] { "teamprojectcollection", "tpc" }, "team project collection uri", false, "https://tfs.healthcare.siemens.com:8090/tfs/ikm.tpc.projects"));
            cmd.Add(new CmdLine.Parameter(new[] { "verbose", "v", "debug", "d" }, "verbose mode", false, "false"));
            if (!cmd.Parse(args))
            {
                cmd.PrintErrors("testrunanalyzer.exe");
                Console.ReadKey();
                return;
            }
            Out.VerbosityLevel = bool.Parse(cmd.ValueOf("verbose")) ? 1 : 0;

            var ioc = new Container();
            ioc.Register(() => new Uri(cmd.ValueOf("tpc")));
            ioc.Register<ITfsTeamProjectCollection, TfsTeamProjectCollectionWrapper>(Lifestyle.Singleton);
            var tpc = ioc.GetInstance<ITfsTeamProjectCollection>();
            ioc.Register(() => tpc.GetService<IBuildServer>());
            ioc.Register<ITestManagementTeamProjectProvider>(() => new TestManagementTeamProjectProvider(tpc));
            var tmtpp = ioc.GetInstance<ITestManagementTeamProjectProvider>();
            ioc.Register(() => tmtpp.GeTestManagementTeamProject(cmd.ValueOf("tp")));
            ioc.Register<TestRunAnalyzer>();

            var a = ioc.GetInstance<TestRunAnalyzer>();
            a.Analyze(cmd.ValueOf("tp"), cmd.ValueOf("build"));

            Console.WriteLine("\n\ndone");
            Console.ReadKey();
        }
    }

    internal class TestRunAnalyzer
    {
        private readonly IBuildServer myBuildServer;
        private readonly ITestManagementTeamProject myTestManagementTeamProject;

        public TestRunAnalyzer(IBuildServer buildServer, ITestManagementTeamProject testManagementTeamProject)
        {
            myBuildServer = buildServer;
            myTestManagementTeamProject = testManagementTeamProject;
        }

        public void Analyze(string teamProjectName, string buildDefinitionName)
        {
            var spec = myBuildServer.CreateBuildDetailSpec(teamProjectName, buildDefinitionName);
            //spec.Status = BuildStatus.PartiallySucceeded | BuildStatus.Failed;
            spec.Status = (BuildStatus.All & ~BuildStatus.InProgress);
            spec.InformationTypes = new[] { "CustomSummaryInformation", "BuildError" };
            spec.MinFinishTime = DateTime.Now.AddDays(-5); //sinceDate;
            spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;

            var builds = myBuildServer.QueryBuilds(spec).Builds;
            foreach (var buildDetail in builds)
            {
                Out.Log(buildDetail.BuildNumber);
                var testRuns = myTestManagementTeamProject.TestRuns.ByBuild(buildDetail.Uri);
            }
        }

    }

}
