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
            var differentAssemblies = data.GroupBy(d => d.Assembly).Count() > 1;
            data.OrderByDescending(d => d.Duration).ToList()
                .ForEach(d =>
                {
                    Out.Info("{0} on {1} for {2}", d.Duration, d.TestRun.DateStarted.ToString("yy-MM-dd HH:mm"), d.Build);
                });
        }
    }
}
