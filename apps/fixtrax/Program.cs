﻿using juba.consoleapp;
using juba.tfs.interfaces;
using juba.tfs.wrappers;
using SimpleInjector;
using System;
using System.IO;
using System.Linq;

using juba.consoleapp.Out;

using sybi;
using CmdLine = juba.consoleapp.CmdLine;

namespace fixtrax
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var output = new ConsoleOut();
            var i = new CmdLine.Interpreter();
            i.Add(new CmdLine.Parameter(new[] { "changeset", "cs" }, "CS id", "changeset to track", false));
            i.Add(new CmdLine.Parameter(new[] { "module", "mb" }, "path", "module branch to find changes (e.G. Core/PCP/v4) on it", false));
            i.Add(new CmdLine.Parameter(new[] { "modules", "file" }, "file name", "a file containing module branches to find changes on there", false));
            i.Add(new CmdLine.Parameter(new[] { "days", "for" }, "count", "find changes of last how many days", false, "2"));
            i.Add(new CmdLine.Parameter(new[] { "target", "ds" }, "path", "target deployment set such Torus/PCP/VA30", true));
            i.Add(new CmdLine.Parameter(new[] { "workitem", "wi", "bi", "defect" }, "BI id", "workitem to track", false));
            i.Add(new CmdLine.Parameter(new[] { "verbose", "v", "debug", "d" }, "bool", "verbose mode", false, "false"));
            if (!i.Parse(string.Join(" ", args)))
            {
                i.PrintErrors("push.exe");
                return;
            }
            output.VerbosityLevel = bool.Parse(i.ValueOf("verbose")) ? 1 : 0;

            var ioc = new Container();
            ioc.Register(() => new Uri("https://tfs.healthcare.siemens.com:8090/tfs/ikm.tpc.projects"), Lifestyle.Singleton);
            ioc.Register<IVersionControlServer, VersionControlServerWrapper>(Lifestyle.Singleton);
            ioc.Register<IWorkItemStore, WorkItemStoreWrapper>(Lifestyle.Singleton);
            ioc.Register<IBranchFinder, BranchFinder>();
            ioc.Register<IVersionInfoFinder, VersionInfoFinder>();
            ioc.Register<ILinkedChangesetsExtractor, LinkedChangesetsExtractor>();
            ioc.Register<FixTrax>();

            var tracker = ioc.GetInstance<FixTrax>();

            try
            {
                var dsName = i.ValueOf("target");

                if (i.IsSpecified("changeset"))
                {
                    var csid = i.Evaluate<int>("changeset", CmdLine.Interpreter.DefaultIntConverter,
                      (x) => { if (x <= 0) throw new Exception("The specified changeset id is not valid"); });
                    tracker.TrackCS(csid, dsName, output);
                    return;
                }

                if (i.IsSpecified("module") || i.IsSpecified("modules"))
                {
                    var days = i.Evaluate<int>("days", CmdLine.Interpreter.DefaultIntConverter,
                      (x) => { if (x <= 0) throw new Exception("The specified number of days is not valid"); });
                    if (i.IsSpecified("module")) tracker.TrackModuleChanges(i.ValueOf("module"), days, dsName, output);
                    else tracker.TrackModuleChangesFromInputFile(i.ValueOf("modules"), days, dsName, output);
                    return;
                }

                if (i.IsSpecified("workitem"))
                {
                    var workitem = i.Evaluate<int>("workitem", CmdLine.Interpreter.DefaultIntConverter,
                      (x) => { if (x <= 0) throw new Exception("The specified workitem id is not valid"); });
                    tracker.TrackWorkitem(workitem, dsName, output);
                    Console.WriteLine("done");
                    Console.ReadKey();
                    return;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("{0} caught: {1}", e.GetType().Name, e.Message);
                output.Log(e.ToString());
            }
        }

    }

    internal class FixTrax
    {
        private readonly IVersionControlServer myVersionControlServer;
        private readonly IWorkItemStore myWorkItemStore;
        private readonly IVersionInfoFinder myVersionInfoFinder;
        private readonly ILinkedChangesetsExtractor myLinkedChangesetsExtractor;

        #region constants
        private static readonly string ServerUri = @"https://tfs.healthcare.siemens.com:8090/tfs/ikm.tpc.projects";

        private static readonly string DSRootPath = @"$/syngo.net/Deploy";
        private static readonly string ModulesRootPath = @"$/syngo.net/Modules";

        #endregion constants

        public FixTrax(IVersionControlServer vcs, IWorkItemStore wis, IVersionInfoFinder vif, ILinkedChangesetsExtractor lce)
        {
            myVersionControlServer = vcs;
            myWorkItemStore = wis;
            myVersionInfoFinder = vif;
            myLinkedChangesetsExtractor = lce;
        }

        internal void TrackWorkitem(int workitem, string dsName, IConsoleAppOut output)
        {
            output.Log("called TrackWorkitem({0}, {1})", workitem, dsName);
            var wi = myWorkItemStore.GetWorkItem(workitem);
            Console.WriteLine(wi.ToString());
            foreach (var cs in myLinkedChangesetsExtractor.GetChangesets(wi).ToList())
            {
                TrackCS(cs.ChangesetId, dsName, output);
            }
        }

        public void TrackModuleChangesFromInputFile(string moduleFile, int days, string dsName, IConsoleAppOut output)
        {
            using (var f = new StreamReader(new FileStream(moduleFile, FileMode.Open, FileAccess.Read)))
            {
                while (!f.EndOfStream)
                {
                    var moduleBranch = f.ReadLine();
                    TrackModuleChanges(moduleBranch, days, dsName, output);
                }
            }
        }

        public void TrackModuleChanges(string moduleBranch, int days, string dsName, IConsoleAppOut output)
        {
            // load history of the module for the specified number of days
            myVersionControlServer.QueryHistory(string.Join("/", ModulesRootPath, moduleBranch), true, true, DateTime.Today - TimeSpan.FromDays(days), false)
                .ToList()
                .ForEach(c =>
                {
                    if (c.WorkItems.Any()) TrackCS(c.ChangesetId, dsName, output);
                });
        }

        public void TrackCS(int csid, string dsAndBranchName, IConsoleAppOut output)
        {
            // idnetify the source control project based on the specified CS
            var cs = myVersionControlServer.GetChangeset(csid);
            if (cs == null) { throw new SybiException("Could not find a changeset with Id {0}", csid); }
            var anItemOfCS = cs.Changes.First().Item.ServerItem;
            var scpFinder = new SourceControlProjectFinder();
            var sourceScp = scpFinder.Find(anItemOfCS);

            // load history of version info file of the module
            // find next version of the module after the asked CS
            var sourceVersion = myVersionInfoFinder.FindByChangeset(cs);
            if (sourceVersion.Item == null)
            {
                PrintTrackingResult(cs, sourceScp.Name, null, dsAndBranchName, null);
                return;
            }
            output.Log("{1} CS {0} identified as subsequent version upload", sourceVersion.Item.ChangesetId, sourceVersion.Item.ServerItem);
            var elapsedTime = sourceVersion.Item.CheckinDate - cs.CreationDate;
            output.Log("{1} {2} (C{3}{4})",
                csid, sourceScp.Name, sourceVersion.Id, sourceVersion.Item.ChangesetId,
                elapsedTime < TimeSpan.FromHours(2) ? string.Format(" +{0}", elapsedTime) : "");

            // load history of the DS version file
            // find next change after the module version arrived the DS
            var dsVersion = myVersionInfoFinder.FindByChangesetId(sourceVersion.Item.ChangesetId,
                PathAndFilenameConventions.AddVersionInfoFolderSubpathTo(string.Join("/", DSRootPath, dsAndBranchName)),
                PathAndFilenameConventions.VersionInfoFileNameOf("Deploy", dsAndBranchName.Split('/', '\\')[0]));
            if (dsVersion.Item == null)
            {
                PrintTrackingResult(cs, sourceScp.Name, sourceVersion.Id, dsAndBranchName, sourceScp.ScpType != "Modules" ? "has this version, but no ds version yet" : null);
                return;
            }

            PrintTrackingResult(cs, sourceScp.Name, sourceVersion.Id, dsAndBranchName, dsVersion.Id);

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

    }

}
