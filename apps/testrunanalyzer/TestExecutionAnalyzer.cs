using System;
using System.Linq;
using System.Text.RegularExpressions;

using juba.consoleapp;

using Microsoft.TeamFoundation.Build.Client;

namespace testrunanalyzer
{
    internal class TestExecutionAnalyzer
    {
        private readonly IBuildServer myBuildServer;
        private readonly ITestExecutionDataCollector myDataCollector;

        public TestExecutionAnalyzer(IBuildServer buildServer, ITestExecutionDataCollector dataCollector)
        {
            myBuildServer = buildServer;
            myDataCollector = dataCollector;
        }

        public void Analyze(string teamProjectName, string buildSpec, int days, int threshold, int peakfilter, bool extendedoutput)
        {
            var spec = myBuildServer.CreateBuildDetailSpec(teamProjectName);
            spec.InformationTypes = null;
            spec.Status = (BuildStatus.All & ~BuildStatus.InProgress);
            new BuildSpecInjector().Inject(buildSpec, ref spec);
            spec.MinFinishTime = DateTime.Today - TimeSpan.FromDays(days - 1);
            spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;

            var data = myDataCollector.CollectData(myBuildServer.QueryBuilds(spec).Builds);
            Console.WriteLine("Top 10:");
            data.OrderByDescending(i => Convert.ToInt32((double)i.Duration.TotalMilliseconds))
                .GroupBy(i => i.Assembly)
                .Where(g => g.Average(r => r.Duration.TotalMinutes) > threshold
                    || g.Count(r => r.Duration.TotalMinutes > threshold) > g.Count() * peakfilter / 100)
                .Take(10)
                .ToList()
                .ForEach(g =>
                {
                    var i = g.OrderByDescending(r => r.Duration.TotalMinutes).ToList();
                    Out.Info("\t{0}\t{1}", i.First(), myDataCollector.GetDropFolder(i.First()));
                    if (extendedoutput) Out.Info("\t\t  ({0})", string.Join(", ", i.Select(r => r.Duration.ToString("g"))));
                });
        }
    }
}