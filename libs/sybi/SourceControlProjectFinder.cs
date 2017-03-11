using System;
using System.Linq;

using juba.tfs.interfaces;


namespace sybi
{
    public class SourceControlProjectFinder : ISourceControlProjectFinder
    {
        private static string[] knownSCPCollectionFolders = { "Deploy", "Modules" };

        internal static string[] KnownSCPCollectionFolders
        {
            get { return knownSCPCollectionFolders; }
        }

        public ISourceControlProject Find(string path)
        {
            var dirs = path.Split('/').ToList();
            var i = dirs.IndexOf(
                        dirs.FirstOrDefault(d => KnownSCPCollectionFolders.Any(f => f.Equals(d, StringComparison.InvariantCultureIgnoreCase))));
            if (i < 0) throw new SybiException("Could not find a known SCP collection folder on the specified path (\"{0}\")", path);
            return new SourceControlProject(dirs[i], dirs[i + 1]);
        }

        public ISourceControlProject Find(IItem item)
        {
            return Find(item.ServerItem);
        }

    }
}
