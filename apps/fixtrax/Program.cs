﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;


namespace fixtrax
{
    internal class Program
    {
        private static void Main(string[] args)
        {
        }

    }

    internal class FixTrax
    {
        private static readonly string ServerUri = @"tfs.healthcare.siemens.com:8090/tfs";
        private static readonly string SourceControlRootPath = @"$/syngo.net";
        private static readonly string VersionInfoFolderPath = @"_Globals/VersionInformation";
        private static readonly string ModuleVersionInfoFilePattern = @"Modules.{0}_ReferenceVersions.xml";
        private static readonly string DeployVersionInfoFilePattern = @"Deploy.{0}_ReferenceVersions.xml";
        private static readonly string DeployPath = @"Deploy/Torus/PCP/VA30";
        private static readonly string ModulesPathPattern = @"Modules/{0}/PCP/v4";

        public void TrackCS(string csparam)
        {
            // parse cmd line
            // - which module
            // - which CS
            int csid;
            if (!int.TryParse(csparam, out csid))
            {
                Console.WriteLine("specified CS id is not valid");
                return;
            }

            // load config data

            // load header data of the specified CS
            var cs = VCS.GetChangeset(csid);
            var moduleName = GetSCPName(cs.Changes.First().Item.ServerItem);

            // load version info of asked module
            var moduleVersions = GetModuleVersion(moduleName);

            // find next version of the module after the asked CS

            // load history of the module version info file in Torus

            // find next change after the version created

            // load history of the Torus version file

            // find next change after the module version arrived Torus
        }

        private static readonly string[] KnownProductAggregationFolders = { "Deploy", "Modules" };
        private string GetSCPName(string path)
        {
            var dirs = path.Split('/');
            if (dirs.Count() < 3)
                throw new FixTraxException("Could not retrieve an SCP name base on \"{0}\"", path);
            if (!KnownProductAggregationFolders.Any(f => f.Equals(dirs[1], StringComparison.InvariantCultureIgnoreCase)))
                throw new FixTraxException("\"{0}\" is not on a known/supported path ({1})", path, string.Join(", ", KnownProductAggregationFolders));
            return dirs[2];
        }

        private VersionControlServer _vcs;

        private VersionControlServer VCS
        {
            get
            {
                if (_vcs != null) return _vcs;
                var server = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(new Uri(ServerUri));
                _vcs = server.GetService<VersionControlServer>();
                return _vcs;
            }
        }
    }

    internal class FixTraxException : Exception
    {
        public FixTraxException(string fmt, params object[] args)
            : base(string.Format(fmt, args))
        {

        }
    }
}
