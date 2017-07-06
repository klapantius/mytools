using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using juba.consoleapp;
using juba.consoleapp.Out;

using Microsoft.TeamFoundation.Build.Client;

namespace testrunanalyzer
{
    internal class TestExecutionAnalyzer
    {
        private readonly IBuildServer myBuildServer;
        private readonly ITestExecutionDataCollector myDataCollector;
        private readonly IConsoleAppOut myOut;

        public TestExecutionAnalyzer(IBuildServer buildServer, ITestExecutionDataCollector dataCollector, IConsoleAppOut @out)
        {
            myBuildServer = buildServer;
            myDataCollector = dataCollector;
            myOut = @out;
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
            var filteredData = data
                .GroupBy(i => i.Assembly)
                .Select(g =>
                {
                    var rawAvg = g.Average(r => r.Duration.TotalMinutes);
                    var median = g.Median(r => r.Duration.TotalMinutes);
                    var refVal = Math.Min(rawAvg, median ?? 0);
                    Console.WriteLine("{0}: rawAvg: {1}, median: {2}, refVal: {3}, skipping {4} of {5} items",
                        g.Key, rawAvg, median, refVal, Math.Min(g.Count(r => r.Duration.TotalMinutes >= refVal), g.Count() / 10), g.Count());
                    return g.Skip(Math.Min(g.Count(r => r.Duration.TotalMinutes >= refVal), g.Count() / 10));
                })
                .OrderByDescending(g => g.Average(i => i.Duration.TotalMilliseconds))
                .Take(10)
                .ToList();
            Console.WriteLine("Top 10:");
            filteredData
                .ForEach(g =>
                {
                    var i = g.OrderByDescending(r => r.Duration.TotalMinutes).ToList();
                    myOut.Info("\t{0}\t{1}", i.First(), myDataCollector.GetDropFolder(i.First()));
                    if (extendedoutput) myOut.Info("\t\t  ({0})", string.Join(", ", i.Select(r => r.Duration.ToString("g"))));
                });
        }

    }

    public static class MedianExtensionClass
    {
        public static double? Median<TColl, TValue>(
            this IEnumerable<TColl> source,
            Func<TColl, TValue> selector)
        {
            return source.Select<TColl, TValue>(selector).Median();
        }

        public static double? Median<T>(
            this IEnumerable<T> source)
        {
            if (Nullable.GetUnderlyingType(typeof(T)) != null)
                source = source.Where(x => x != null);

            int count = source.Count();
            if (count == 0)
                return null;

            source = source.OrderBy(n => n);

            int midpoint = count / 2;
            if (count % 2 == 0)
                return (Convert.ToDouble(source.ElementAt(midpoint - 1)) + Convert.ToDouble(source.ElementAt(midpoint))) / 2.0;
            else
                return Convert.ToDouble(source.ElementAt(midpoint));
        }

    }
}