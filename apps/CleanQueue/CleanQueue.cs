using juba.consoleapp.CmdLine;
using juba.consoleapp.Out;
using juba.tfs.interfaces;
using juba.tfs.wrappers;
using Microsoft.TeamFoundation.Build.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace CleanQueue
{
    class CleanQueue
    {
        static void Main(string[] args)
        {
            var ioc = new Container();

            ioc.Register<IConsoleAppOut, ConsoleOut>(Lifestyle.Singleton);
            ioc.Register<ICmdLineInterpreter, Interpreter>(Lifestyle.Singleton);
            ioc.Register(() => new Uri(ioc.GetInstance<ICmdLineInterpreter>().ValueOf("tpc")), Lifestyle.Singleton);
            ioc.Register<ITfsTeamProjectCollection, TfsTeamProjectCollectionWrapper>(Lifestyle.Singleton);
            ioc.Register(() => ioc.GetInstance<ITfsTeamProjectCollection>().GetService<IBuildServer>());
            ioc.Register<IVersionControlServer, VersionControlServerWrapper>(Lifestyle.Singleton);

            var myOut = ioc.GetInstance<IConsoleAppOut>();
            var cmd = ioc.GetInstance<ICmdLineInterpreter>();

            cmd.Add(new Parameter(new[] { "build", "b" }, "name", "build definition name", false));
            cmd.Add(new Parameter(new[] { "teamproject", "tp" }, "name", "team project name", false, "syngo.net"));
            cmd.Add(new Parameter(new[] { "teamprojectcollection", "tpc" }, "uri", "team project collection uri", false, "https://tfs.healthcare.siemens.com:8090/tfs/ikm.tpc.projects"));
            cmd.Add(new Parameter(new[] { "anykey" }, "bool", "doesn't exit at the end", false, "false"));

            cmd.Add(new Command(new[] { "list" }, "list build requests to the asked build definition", () =>
            {
                cmd.Parse(args);
                var vcs = ioc.GetInstance<IVersionControlServer>();
                var bs = ioc.GetInstance<IBuildServer>();

                myOut.Info($"Requesting queue information for {cmd.ValueOf("teamproject")} / {cmd.ValueOf("build")} ...");
                IQueuedBuildSpec qbSpec = bs.CreateBuildQueueSpec(cmd.ValueOf("teamproject"), cmd.ValueOf("build"));
                IQueuedBuildQueryResult qbResults = bs.QueryQueuedBuilds(qbSpec);

                var toBeCancelled = new List<int>();
                var shelvesets = new List<ShelvesetInfo>();
                var refRequests = new Dictionary<string, List<int>>();
                foreach (IQueuedBuild qb in qbResults.QueuedBuilds)
                {
                    var s = new ShelvesetInfo(vcs, qb);
                    shelvesets.Add(s);
                    myOut.Info($"{qb.Id}\t{qb.RequestedByDisplayName}\t{qb.ShelvesetName}\t\"{s.Comment}\"");
                    s.Changes.ToList().ForEach(c =>
                    {
                        var f = c.FileName;
                        if (f.Contains("ReferenceVersions.xml"))
                        {
                            if (!refRequests.ContainsKey(f)) refRequests.Add(f, new List<int>());
                            refRequests[f].Add(qb.Id);
                            var li = vcs.GetItem(c.ServerItem);
                            var latestVersion = li != null ? VersionFromReferenceFile(li.DownloadFile()) : "n/a";
                            var fromVersion = VersionFromReferenceFile(c.DownloadBaseFile());
                            var toVersion = VersionFromReferenceFile(c.DownloadShelvedFile());
                            var errorMessage = string.Empty;
                            if (fromVersion != latestVersion && toVersion != latestVersion) errorMessage = $"merge conflict expected, because latest version is {latestVersion}";
                            myOut.Info($"\t{f,-53} : {fromVersion} --> {toVersion} {errorMessage}");
                        }
                    });
                }
                refRequests.ToList().ForEach(rc =>
                {
                    var requestsWithThisFileOnly = rc.Value.Where(r => shelvesets.Single(s => s.RequestId == r).Changes.Count() == 1).OrderBy(r => r);
                    var c = requestsWithThisFileOnly.Count();
                    if (c > 1) toBeCancelled.AddRange(requestsWithThisFileOnly.Take(c - 1));
                });
                if (toBeCancelled.Count() > 0)
                {
                    myOut.Info($"{Environment.NewLine}You can cancel these requests:");
                    toBeCancelled.OrderBy(r => r).ToList().ForEach(r =>
                    {
                        var sh = shelvesets.Single(s => s.RequestId == r);
                        var push = sh != null && sh.Changes.Any() ? sh.Changes.First().FileName : "???";
                        var kept = refRequests[push].Last();
                        myOut.Info($"\t{r} - {push} because {kept} is newer");
                    });
                }
                else myOut.Info("No request can be cancelled.");
            })).
            Requires("build");

            myOut.VerbosityLevel = cmd.Evaluate("verbose", Interpreter.DefaultBoolConverter) ? 1 : 0;

            cmd.ExecuteCommands();

            if (cmd.Evaluate("anykey", Interpreter.DefaultBoolConverter))
            {
                Console.WriteLine("\n\ndone, please press a key to finish the execution");
                Console.ReadKey();
            }

        }

        static string VersionFromReferenceFile(Stream stream)
        {
            var d = XDocument.Load(stream);
            var version = d.Descendants().FirstOrDefault(p => p.Name.LocalName == "Version");
            return version != null ? version.Value : "???";
        }

        private class ShelvesetInfo
        {
            public ShelvesetInfo(IVersionControlServer vcs, IQueuedBuild qb)
            {
                this.vcs = vcs;
                RequestId = qb.Id;
                ShelvesetName = qb.ShelvesetName.Split(';')[0];
                Owner = qb.ShelvesetName.Split(';')[1];
                myShelveset = vcs.QueryShelvesets(ShelvesetName, Owner).FirstOrDefault();
                Comment = myShelveset != null ? myShelveset.Comment : "shelveset could not be found";
                var pendingSet = vcs.QueryShelvedChanges(ShelvesetName, Owner).FirstOrDefault();
                Changes = pendingSet != null ? pendingSet.PendingChanges : new PendingChange[] { };
            }
            private readonly IVersionControlServer vcs;
            public int RequestId { get; }
            public List<string> Modules { get; }
            public bool HasAdditionalChanges { get; set; }
            public string ShelvesetName { get; }
            public string Owner { get; }
            private readonly Shelveset myShelveset;
            public string Comment { get; }
            public PendingChange[] Changes { get; }
        }
    }
}
