using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

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
            ioc.Register(() => new Uri(cmd.ValueOf("tpc")), Lifestyle.Singleton);
            ioc.Register<ITfsTeamProjectCollection, TfsTeamProjectCollectionWrapper>(Lifestyle.Singleton);
            ioc.Register(() => ioc.GetInstance<ITfsTeamProjectCollection>().GetService<IBuildServer>());
            ioc.Register<ITestManagementTeamProjectProvider, TestManagementTeamProjectProvider>();
            ioc.Register(() => ioc.GetInstance<ITestManagementTeamProjectProvider>().GeTestManagementTeamProject(cmd.ValueOf("tp")));
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
            spec.InformationTypes = null;
            spec.MinFinishTime = DateTime.Now.AddDays(-1); //sinceDate;
            spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;

            var data = CollectData(myBuildServer.QueryBuilds(spec).Builds);
            Console.WriteLine("Top 10:");
            data.OrderByDescending(i => Convert.ToInt32(i.duration.TotalMilliseconds))
                .ToList()
                .Take(10).ToList().ForEach(i => Out.Info("\t{0}", i));
        }

        private struct TestExecutionData
        {
            public string build;
            public string assembly;
            public TimeSpan duration;
            public string dropFolder;
            public override string ToString()
            {
                return string.Format("{0} {1} {2}", duration, assembly, dropFolder);
            }
        }

        private List<TestExecutionData> CollectData(IBuildDetail[] builds)
        {
            var result = new List<TestExecutionData>();
            foreach (var buildDetail in builds)
            {
                Out.Log(buildDetail.BuildNumber);
                var testRuns = myTestManagementTeamProject.TestRuns.ByBuild(buildDetail.Uri);
                foreach (var testRun in testRuns)
                {
                    var item= new TestExecutionData()
                    {
                        build = buildDetail.BuildNumber,
                        assembly = testRun.Title.Substring(0, testRun.Title.LastIndexOf(".dll", StringComparison.InvariantCultureIgnoreCase)),
                        duration = testRun.DateCompleted - testRun.DateStarted,
                        dropFolder = Path.Combine(Path.GetDirectoryName(buildDetail.LogLocation), GetRelativeOutputPath(testRun)),
                    };
                    result.Add(item);
                    Out.Log("\t{0} {1} {2}", item.duration, item.assembly, item.dropFolder);
                }
            }
            return result;
        }

        private static readonly string myByteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
        private static string GetRelativeOutputPath(ITestRun testRun)
        {
            var x = testRun.Attachments.FirstOrDefault(foo => foo.AttachmentType == "TmiTestRunSummary");
            if (x == null) return string.Empty;
            
            var array = new byte[99999999];
            x.DownloadToArray(array, 0);
            var txt = Encoding.UTF8.GetString(array).TrimEnd('\0');
            if (txt.StartsWith(myByteOrderMarkUtf8))
            {
                txt = txt.Remove(0, myByteOrderMarkUtf8.Length);
            }
            var d = XDocument.Parse(txt);
            if (d.Root == null) return string.Empty;
            
            var node = d.Descendants()
                .FirstOrDefault(foo => foo.Attributes()
                    .Any(a => a.Name.LocalName == "name" && a.Value == "RelativeOutputPath"));
            if (node == null) return string.Empty;

            var attr = node.Attributes().FirstOrDefault(foo => foo.Name.LocalName == "value");
            return attr == null ? string.Empty : attr.Value;
        }
    }

}
