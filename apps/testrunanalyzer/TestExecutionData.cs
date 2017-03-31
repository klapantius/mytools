using System;

using Microsoft.TeamFoundation.TestManagement.Client;


namespace testrunanalyzer
{
    public struct TestExecutionData
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
        public TimeSpan Duration
        {
            get
            {
                return TestRun != null ?
                    TestRun.DateCompleted - TestRun.DateStarted :
                    new TimeSpan(0);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}:{1:D2}:{2:D2} - {3}", Duration.Hours, Duration.Minutes, Duration.Seconds, Assembly);
        }
    }
}