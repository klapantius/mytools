using juba.tfs.interfaces;
using juba.tfs.wrappers;
using System;

namespace sybi
{
    public static class TfsAccess
    {
        private static IVersionControlServer myVCS;
        public static IVersionControlServer VCS
        {
            get
            {
                if (myVCS == null) { myVCS = new VersionControlServerWrapper(new Uri(Configuration.Data.TFSUri)); }
                return myVCS;
            }
            internal set { myVCS = value; }
        }
    }
}
