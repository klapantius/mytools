using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tfsaccess;

namespace sybi
{
    public interface IBranch
    {
        IEnumerable<IVersionInfo> LatestVersionInfo { get; }
    }
    public class Branch : IBranch
    {
        private string myBranchPath;
        private string myScpPath;
        public string Error { get; internal set; }
        public Branch(string scpPath, string branchPath)
        {
            myScpPath = scpPath;
            myBranchPath = branchPath;
        }

        public IEnumerable<IVersionInfo> LatestVersionInfo
        {
            get
            {
                //var versionInfoItems = TFSAccess.VCS.GetItems(Path.Combine(scpPath, branchPath, TFSAccess.Configuration.VersionInfoPath));
                throw new NotImplementedException();
            }
        }
    }
}
