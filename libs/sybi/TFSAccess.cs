using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using tfsaccess;

namespace sybi
{
    public static class TFSAccess
    {
        private static ConfigurationData myConfiguration;
        internal static ConfigurationData Configuration
        {
            get
            {
                if (myConfiguration == null)
                {
                    var cfg = new Configuration();
                    myConfiguration = cfg.Data;
                }
                return myConfiguration;
            }
        }
        private static IVersionControlServer myVCS;
        public static IVersionControlServer VCS
        {
            get
            {
                if (myVCS == null) { myVCS = new VersionControlServerWrapper(Configuration.TFSUri); }
                return myVCS;
            }
            internal set { myVCS = value; }
        }
    }
}
