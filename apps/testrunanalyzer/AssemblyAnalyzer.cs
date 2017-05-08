using System;
using System.Linq;
using System.Text.RegularExpressions;

using juba.consoleapp;

using Microsoft.TeamFoundation.Build.Client;


namespace testrunanalyzer
{
    public class AssemblyAnalyzer
    {
        private readonly IBuildServer myBuildServer;
        private readonly ITestExecutionDataCollector myDataCollector;

        public AssemblyAnalyzer(IBuildServer buildServer, ITestExecutionDataCollector dataCollector)
        {
            myBuildServer = buildServer;
            myDataCollector = dataCollector;
        }

        public void Analyze(string teamProjectName, string buildSpec, int days, string assemblyspec)
        {
            var spec = myBuildServer.CreateBuildDetailSpec(teamProjectName);
            spec.InformationTypes = null;
            spec.Status = (BuildStatus.All & ~BuildStatus.InProgress);
            new BuildSpecInjector().Inject(buildSpec, ref spec);
            spec.MinFinishTime = DateTime.Today - TimeSpan.FromDays(days - 1);
            spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;

            var result = myDataCollector.CollectData(myBuildServer.QueryBuilds(spec).Builds);
            var asss = new Regex(assemblyspec, RegexOptions.IgnoreCase);
            var data = result.Where(d => asss.IsMatch(d.Assembly)).ToList();
            var groupedByAssemblies = data.GroupBy(d => d.Assembly).ToList();
            groupedByAssemblies.ForEach(a =>
            {
                Out.Info("{0} ({1} results)", a.Key, a.Count());
                var max = a.Max(r => r.Duration.TotalSeconds);
                var min = a.Min(r => r.Duration.TotalSeconds);
                var tenPrc = (max - min) / 10;
                a.ToList().ForEach(r =>
                {
                    Out.Info("{2,10}- {0} {1}", r.DurationAsString, r.BuildLogLocation, new string('-', (int)((r.Duration.TotalSeconds-min)/tenPrc)));
                });
            });
        }
    }
}
