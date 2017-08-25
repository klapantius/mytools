using System;
using System.IO;
using System.Linq;

using juba.consoleapp;
using juba.consoleapp.Out;


namespace rsfainstanalyzer
{
    public class LogIterator : ILogIterator
    {
        private string[] myDirNames;
        private string myLogName;
        private IConsoleAppOut myOutput;

        public LogIterator(IConsoleAppOut output)
        {
            myOutput = output;
        }

        public void Process(string path, string logname, int days, Action<TextReader, string> fileLevelAction, Action<string> directoryLevelAction = null)
        {
            var root = Path.GetDirectoryName(path);
            var dirPattern = path.Substring(root.Length + 1);
            var minDay = days > 0 ? DateTime.Today - TimeSpan.FromDays(days) : DateTime.MinValue;
            myDirNames = Directory.GetFileSystemEntries(root, dirPattern, SearchOption.TopDirectoryOnly).
                Where(d => Directory.GetCreationTime(d) > minDay).
                OrderBy(d => d.Substring(0, d.LastIndexOf('.'))).ThenBy(d => int.Parse(d.Substring(d.LastIndexOf('.') + 1))).
                ToArray();
            myLogName = logname;
            myOutput.Info("Processing {0} build results...", myDirNames.Length);
            myDirNames.ToList().ForEach(d =>
            {
                var fnames = Directory.GetFileSystemEntries(d, myLogName, SearchOption.AllDirectories);
                fnames.ToList().ForEach(f =>
                {
                    try
                    {
                        using (var input = new StreamReader(f))
                        {
                            myOutput.Log(f);
                            fileLevelAction(input, d);
                        }
                    }
                    catch (IOException)
                    {
                    }
                });
                if (directoryLevelAction != null) directoryLevelAction(d);
            });
        }
    }
}