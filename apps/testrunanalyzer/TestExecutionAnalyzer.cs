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

        public void Analyze(string teamProjectName, string buildSpec, int days)
        {
            var spec = myBuildServer.CreateBuildDetailSpec(teamProjectName);
            spec.InformationTypes = null;
            spec.Status = (BuildStatus.All & ~BuildStatus.InProgress);
            if (new Regex(@"\d$").IsMatch(buildSpec))
            {
                spec.BuildNumber = buildSpec;
            }
            else
            {
                spec.DefinitionSpec.Name = buildSpec;
                spec.MinFinishTime = DateTime.Today - TimeSpan.FromDays(days - 1);
            }
            spec.QueryOrder = BuildQueryOrder.FinishTimeDescending;

            var data = myDataCollector.CollectData(myBuildServer.QueryBuilds(spec).Builds);
            Console.WriteLine("Top 10:");
            data.OrderByDescending(i => Convert.ToInt32((double)i.Duration.TotalMilliseconds))
                .GroupBy(i => i.Assembly)
                .Where(g => g.Count() > 1)
                .Take(10)
                .ToList()
                .ForEach(i =>
                {
                    Out.Info("\t{0}\t{1}", i.First(), myDataCollector.GetDropFolder(i.First()));
                    Out.Log("\t\t  ({0})", string.Join(", ", i.Select(r => r.Duration.ToString("g"))));
                });
        }
    }
}