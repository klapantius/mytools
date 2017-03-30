using System;


namespace testrunanalyzer
{
    public struct TestExecutionData
    {
        public string build;
        public string buildDefinition;
        public string assembly;
        public TimeSpan duration;
        public string dropFolder;
        public override string ToString()
        {
            return string.Format("{0} {1} {2}", duration, assembly, dropFolder);
        }
    }
}