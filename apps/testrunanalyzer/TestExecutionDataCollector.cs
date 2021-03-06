using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using juba.consoleapp.Out;

using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.TestManagement.Client;


namespace testrunanalyzer
{
    public interface ITestExecutionDataCollector
    {
        List<TestExecutionData> CollectData(IBuildDetail[] builds);
        List<TestExecutionData> CollectData(IBuildDetail[] builds, Regex assemblySpec);
        string GetDropFolder(TestExecutionData data);
    }

    public class TestExecutionDataCollector : ITestExecutionDataCollector
    {
        private readonly ITestManagementTeamProject myTestManagementTeamProject;
        private readonly IConsoleAppOut myOutput;

        public TestExecutionDataCollector(ITestManagementTeamProject testManagementTeamProject, IConsoleAppOut output)
        {
            myTestManagementTeamProject = testManagementTeamProject;
            this.myOutput = output;
        }

        public List<TestExecutionData> CollectData(IBuildDetail[] builds)
        {
            return CollectData(builds, null);
        }

        public List<TestExecutionData> CollectData(IBuildDetail[] builds, Regex assemblySpec)
        {
            var result = new List<TestExecutionData>();
            foreach (var buildDetail in builds)
            {
                myOutput.Log(buildDetail.BuildNumber);
                var testRuns = myTestManagementTeamProject.TestRuns.ByBuild(buildDetail.Uri);
                foreach (var testRun in testRuns)
                {
                    var item = new TestExecutionData()
                    {
                        Build = buildDetail.BuildNumber,
                        BuildDefinition = buildDetail.BuildDefinition.Name,
                        TestRun = testRun,
                        BuildLogLocation = Path.GetDirectoryName(buildDetail.LogLocation),
                    };
                    if (assemblySpec==null || assemblySpec.IsMatch(item.Assembly))
                    {
                        var testResults = testRun.QueryResults();
                        testResults.OrderBy(r => r.DateStarted).FirstOrDefault();
                        result.Add(item);
                        myOutput.Log("\t{0} {1}", item.Duration.ToString("g"), item.Assembly);
                    }
                }
            }
            return result;
        }

        public string GetDropFolder(TestExecutionData data)
        {
            return Path.Combine(data.BuildLogLocation, GetRelativeOutputPath(data.TestRun));
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