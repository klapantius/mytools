using System;
using System.IO;
using System.Linq;

using juba.consoleapp;


namespace rsfainstanalyzer
{
    public class LogIterator : ILogIterator
    {
        private string[] myDirNames;
        private string myLogName;

        public void Process(string path, string logname, int days, Action<TextReader, string> fileLevelAction, Action<string> directoryLevelAction = null)
        {
            var root = Path.GetDirectoryName(path);
            var dirPattern = path.Substring(root.Length + 1);
            var minDay = days > 0 ? DateTime.Today - TimeSpan.FromDays(days) : DateTime.MinValue;
            myDirNames = Directory.GetFileSystemEntries(root, dirPattern, SearchOption.TopDirectoryOnly).
                Where(d => Directory.GetCreationTime(d) > minDay).
                ToArray();
            myLogName = logname;
            Out.Info("Processing {0} build results...", myDirNames.Length);
            myDirNames.ToList().ForEach(d =>
            {
                var fnames = Directory.GetFileSystemEntries(d, myLogName, SearchOption.AllDirectories);
                fnames.ToList().ForEach(f =>
                {
                    try
                    {
                        using (var input = new StreamReader(f))
                        {

                            Out.Log(f);
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