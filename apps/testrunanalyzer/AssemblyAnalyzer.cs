using System;
using System.Linq;
using System.Text.RegularExpressions;

using juba.consoleapp;
using juba.consoleapp.Out;

using Microsoft.TeamFoundation.Build.Client;


namespace testrunanalyzer
{
    public class AssemblyAnalyzer
    {
        private readonly IBuildServer myBuildServer;
        private readonly ITestExecutionDataCollector myDataCollector;
        private readonly IConsoleAppOut myOut;

        public AssemblyAnalyzer(IBuildServer buildServer, ITestExecutionDataCollector dataCollector, IConsoleAppOut @out)
        {
            myBuildServer = buildServer;
            myDataCollector = dataCollector;
            myOut = @out;
        }

        public void Analyze(string teamProjectName, string buildSpec, int days, string assemblyspec)
        {
            var spec = myBuildServer.CreateBuildDetailSpec(teamProjectName);
            spec.InformationTypes = null;
            spec.Status = (BuildStatus.All & ~BuildStatus.InProgress);
            new BuildSpecInjector().Inject(buildSpec, ref spec);
            spec.MinFinishTime = DateTime.Today - TimeSpan.FromDays(days - 1);
            spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;

            var asss = new Regex(assemblyspec, RegexOptions.IgnoreCase);
            var result = myDataCollector.CollectData(myBuildServer.QueryBuilds(spec).Builds, asss);
            var data = result.Where(d => asss.IsMatch(d.Assembly)).ToList();
            var groupedByAssemblies = data.GroupBy(d => d.Assembly).ToList();
            groupedByAssemblies.ForEach(a =>
            {
                myOut.Info("{0} ({1} results)", a.Key, a.Count());
                var max = a.Max(r => r.Duration.TotalSeconds);
                var min = a.Min(r => r.Duration.TotalSeconds);
                var tenPrc = (max - min) / 10;
                a.ToList().ForEach(r =>
                {
                    myOut.Info("{2,10} {0} {1}", r.DurationAsString, myDataCollector.GetDropFolder(r), new string('-', Math.Max((int)((r.Duration.TotalSeconds - min) / tenPrc), 1)));
                });
            });
        }
    }
}
