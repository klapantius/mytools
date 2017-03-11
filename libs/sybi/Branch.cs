using System;
using System.Collections.Generic;

namespace sybi
{
    public class Branch : IBranch
    {
        private string myBranchPath;
        public Branch(string path)
        {
            myBranchPath = path;
        }

        public string Path
        {
            get { return myBranchPath; }
        }

        public IEnumerable<IVersionInfo> LatestVersionInfo
        {
            get
            {
                //var versionInfoItems = juba.tfs.wrappers.VCS.GetItems(Path.Combine(scpPath, branchPath, juba.tfs.wrappers.Configuration.VersionInfoPath));
                throw new NotImplementedException();
            }
        }

    }
}
