using System;
using System.Linq;

using juba.consoleapp;

using Microsoft.TeamFoundation.TestManagement.Client;


namespace testrunanalyzer
{
    public class TestExecutionData
    {
        public string Build;
        public string BuildDefinition;
        public string BuildLogLocation;
        public ITestRun TestRun { get; set; }
        public string Assembly
        {
            get
            {
                return TestRun != null && TestRun.Title.Contains(".dll") ?
                    TestRun.Title.Substring(0, TestRun.Title.LastIndexOf(".dll", StringComparison.InvariantCultureIgnoreCase)) :
                    "n/a";
            }
        }

        private TimeSpan myDuration = TimeSpan.MinValue;
        public TimeSpan Duration
        {
            get
            {
                if (myDuration != TimeSpan.MinValue) return myDuration;
                if (TestRun != null)
                {
                    var testResults = TestRun.QueryResults(false);
                    if (testResults != null)
                    {
                        var start = testResults.OrderBy(r => r.DateStarted).First().DateStarted;
                        if (start != DateTime.MinValue)
                            myDuration = TestRun.DateCompleted - start.ToUniversalTime();
                    }
                }
                if (myDuration == TimeSpan.MinValue) myDuration = TestRun != null ? TestRun.DateCompleted - TestRun.DateStarted : TimeSpan.Zero;
                return myDuration;
            }
        }

        public string DurationAsString { get { return string.Format("{0}:{1:D2}:{2:D2}", Duration.Hours, Duration.Minutes, Duration.Seconds); } }

        public override string ToString()
        {
            return string.Format("{0} - {1}", DurationAsString, Assembly);
        }
    }
}