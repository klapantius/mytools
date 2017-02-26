﻿using consoleapp;
using juba.tfs.interfaces;
using juba.tfs.wrappers;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CmdLine = consoleapp.CmdLine;

namespace fixtrax
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var tracker = new FixTrax();

            var i = new CmdLine.Interpreter();
            i.Add(new CmdLine.Parameter(new[] { "changeset", "cs" }, "changeset to track", false));
            i.Add(new CmdLine.Parameter(new[] { "module", "mb" }, "module branch to find changes (e.G. Core/PCP/v4) on it", false));
            i.Add(new CmdLine.Parameter(new[] { "modules", "file" }, "a file containing module branches to find changes on there", false));
            i.Add(new CmdLine.Parameter(new[] { "days", "for" }, "find changes of last how many days", false, "2"));
            i.Add(new CmdLine.Parameter(new[] { "target", "ds" }, "target deployment set such Torus/PCP/VA30", true));
            i.Add(new CmdLine.Parameter(new[] { "workitem", "wi", "bi", "defect" }, "workitem to track", false));
            i.Add(new CmdLine.Parameter(new[] { "verbose", "v", "debug", "d" }, "verbose mode", false, "false"));
            if (!i.Parse(string.Join(" ", args)))
            {
                i.PrintErrors("push.exe");
                return;
            }
            Out.VerbosityLevel = bool.Parse(i.ValueOf("verbose")) ? 1 : 0;

            try
            {
                var dsName = i.ValueOf("target");

                if (i.IsSpecified("changeset"))
                {
                    var csid = i.Evaluate<int>("changeset", CmdLine.Interpreter.DefaultIntConverter,
                      (x) => { if (x <= 0) throw new Exception("The specified changeset id is not valid"); });
                    tracker.TrackCS(csid, dsName);
                    return;
                }

                if (i.IsSpecified("module") || i.IsSpecified("modules"))
                {
                    var days = i.Evaluate<int>("days", CmdLine.Interpreter.DefaultIntConverter,
                      (x) => { if (x <= 0) throw new Exception("The specified number of days is not valid"); });
                    if (i.IsSpecified("module")) tracker.TrackModuleChanges(i.ValueOf("module"), days, dsName);
                    else tracker.TrackModuleChangesFromInputFile(i.ValueOf("modules"), days, dsName);
                    return;
                }

                if (i.IsSpecified("workitem"))
                {
                    var workitem = i.Evaluate<int>("workitem", CmdLine.Interpreter.DefaultIntConverter,
                      (x) => { if (x <= 0) throw new Exception("The specified workitem id is not valid"); });
                    tracker.TrackWorkitem(workitem, dsName);
                    return;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("{0} caught: {1}", e.GetType().Name, e.Message);
                Out.Log(e.ToString());
            }
        }

    }

    internal class FixTrax
    {
        #region constants
        private static readonly string ServerUri = @"https://tfs.healthcare.siemens.com:8090/tfs/ikm.tpc.projects";

        private static readonly string DSRootPath = @"$/syngo.net/Deploy";
        private static readonly string ModulesRootPath = @"$/syngo.net/Modules";
        private static readonly string VersionInfoFolderPath = @"_Globals/VersionInformation";
        private static readonly string VersionInfoFilePattern = @"{0}.{1}_ReferenceVersions.xml";

        private static string[] knownSCPCollectionFolders = { "Deploy", "Modules" };
        internal string[] KnownSCPCollectionFolders
        {
            get { return knownSCPCollectionFolders; }
            set { knownSCPCollectionFolders = value; }
        }
        #endregion constants

        internal void TrackWorkitem(int workitem, string dsName)
        {
            Out.Log("called TrackWorkitem({0}, {1})", workitem, dsName);
            var wi = WIS.GetWorkItem(workitem);
            Console.WriteLine(wi.ToString());
            foreach (var cs in wi.LinkedChangesets(vcs).ToList())
            {
                TrackCS(cs.ChangesetId, dsName);
            }
        }

        public void TrackModuleChangesFromInputFile(string moduleFile, int days, string dsName)
        {
            using (var f = new StreamReader(new FileStream(moduleFile, FileMode.Open, FileAccess.Read)))
            {
                while (!f.EndOfStream)
                {
                    var moduleBranch = f.ReadLine();
                    TrackModuleChanges(moduleBranch, days, dsName);
                }
            }
        }

        public void TrackModuleChanges(string moduleBranch, int days, string dsName)
    {
      // load history of the module for the specified number of days
      VCS.QueryHistory(string.Join("/", ModulesRootPath, moduleBranch), true, true, DateTime.Today - TimeSpan.FromDays(days), false)
          .ToList()
          .ForEach(c =>
          {
            if (c.WorkItems.Any()) TrackCS(c.ChangesetId, dsName);
          });
    }

        public void TrackCS(int csid, string dsName)
        {
            // load config data

            // idnetify the module based on the specified CS
            var cs = VCS.GetChangeset(csid);
            Out.Log(cs.ToString());
            var anItemOfCS = cs.Changes.First().Item.ServerItem;
            var scpName = GetSCPName(anItemOfCS);
            var scpVersionFolder = string.Join("/", GetBranchRootPath(anItemOfCS), VersionInfoFolderPath);
            var scpType = scpVersionFolder.Split('/')[2];
            var scpVersionFileName = string.Format(VersionInfoFilePattern, scpType, scpName);

            // load history of version info file of the module
            // find next version of the module after the asked CS
            var moduleChange = FindVersionOf(cs, scpVersionFolder, scpVersionFileName);
            if (moduleChange == null)
            {
                PrintTrackingResult(cs, scpName, null, dsName, null);
                return;
            }
            Out.Log("{2}/{1} CS {0} identified as subsequent version upload", moduleChange.Item.ChangesetId, scpVersionFileName, scpVersionFolder);
            var moduleVersion = GetModuleVersion(moduleChange.Item);
            var elapsedTime = moduleChange.Item.CheckinDate - cs.CreationDate;
            Out.Log("{1} {2} (C{3}{4})",
                csid, scpName, moduleVersion, moduleChange.Item.ChangesetId,
                elapsedTime < TimeSpan.FromHours(2) ? string.Format(" +{0}", elapsedTime) : "");

            if (scpType == "Modules")
            {
                // load history of the module version info file in Torus
                // find next change after the version created
                var modulePush = FindVersionOf(
                    VCS.GetChangeset(moduleChange.Item.ChangesetId),
                    string.Join("/", DSRootPath, dsName, VersionInfoFolderPath),
                    scpVersionFileName);
                if (modulePush == null)
                {
                    PrintTrackingResult(cs, scpName, moduleVersion, dsName, null);
                    return;
                }
                var modulVersionInTorus = GetModuleVersion(modulePush.Item);
            }

            // load history of the DS version file
            // find next change after the module version arrived the DS
            var dsChange = FindVersionOf(
                VCS.GetChangeset(moduleChange.Item.ChangesetId),
                string.Join("/", DSRootPath, dsName, VersionInfoFolderPath),
                string.Format(VersionInfoFilePattern, "Deploy", dsName.Split('/', '\\')[0]));
            string dsVersion = "has this version, but no ds version yet";
            if (dsChange != null) dsVersion = GetModuleVersion(dsChange.Item);

            PrintTrackingResult(cs, scpName, moduleVersion, dsName, dsVersion);

        }

        private void PrintTrackingResult(IChangeset cs, string scpName, string moduleVersion, string dsName, string dsVersion)
        {
            Console.Write("CS {0} BI:[ {1} ] ==> ", cs.ChangesetId, string.Join(" ", cs.WorkItems.Select(wi => wi.Id)));
            if (string.IsNullOrEmpty(moduleVersion))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("No subsequent {0} version found", scpName);
                Console.ResetColor();
                return;
            }
            Console.Write("{0} {1} ==> ", scpName, moduleVersion);
            if (
                    string.IsNullOrEmpty(dsVersion))
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("not referenced yet by {0}", dsName);
                Console.ResetColor();
                return;
            }
            Console.WriteLine("{0} {1}", dsName.Split('/', '\\')[0], dsVersion);
        }

        internal string GetModuleVersion(IItem item)
        {
            var xd = XDocument.Load(item.DownloadFile());
            return xd
                .Descendants().ToList()
                .First(d => d.Name.LocalName == "Version")
                .Value;
        }

        internal IChange FindVersionOf(IChangeset cs, string vfolder, string vfile)
        {
            Out.Log("Looking for CSs of {0}/{1} not older than {2}", vfolder, vfile, cs.ChangesetId);
            var changesets = VCS.QueryHistory(string.Join("/", vfolder, vfile), false, true, cs, true).ToList();
            Out.Log("Found {0} CSs", changesets.Count());
            var versioncs = changesets.FirstOrDefault(c => c.ChangesetId > cs.ChangesetId);
            if (versioncs == null) { Out.Log("no newer CS found"); return null; }
            var result = versioncs.Changes.SingleOrDefault(c => c.Item.ServerItem.EndsWith(vfile, StringComparison.InvariantCultureIgnoreCase));
            Out.Log("CS identified as next version change: {0} ", versioncs.ChangesetId,
              result != null ? "" :
              string.Format("the first newer CS is not a good found, 'cos it doesn't change {0} :(", vfile));
            return result;
        }

        internal string GetSCPName(string path)
        {
            var dirs = path.Split('/');
            int i = int.MinValue;
            try
            {
                i = dirs
                    .ToList()
                    .IndexOf(dirs
                        .FirstOrDefault(d => KnownSCPCollectionFolders.Any(f => f.Equals(d, StringComparison.InvariantCultureIgnoreCase))));
                if (i < 0) throw new FixTraxException("Could not find known SCP collection folder on the specified path (\"{0}\")", path);
                return dirs[i + 1];
            }
            catch (Exception e) { Console.WriteLine("{0} caught: \"{1}\"", e.GetType().Name, e.Message); }
            throw new FixTraxException("Could not match known parts on the specified path (\"{0}\")", path);
        }

        internal string GetBranchRootPath(string path)
        {
            var item = VCS.GetItem(path);
            if (item.IsBranch) return item.ServerItem;
            return GetBranchRootPath(ParentPathOf(path));
        }

        internal string ParentPathOf(string path)
        {
            return path.Substring(0, path.Trim('/').LastIndexOf('/')).Trim('/');
        }

        private IExtendedVersionControlServer vcs;
        internal IExtendedVersionControlServer VCS
        {
            get
            {
                if (vcs == null) vcs = new VersionControlServerWrapper(new Uri(ServerUri));
                return vcs;
            }
            set { vcs = value; }
        }

        private IWorkItemStore wis;
        internal IWorkItemStore WIS
        {
            get
            {
                if (wis == null) wis = new WorkItemStoreWrapper(new Uri(ServerUri));
                return wis;
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
