﻿using System.IO;

﻿using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using tfsaccess;
using CmdLine = consoleapp.CmdLine;

[assembly: InternalsVisibleTo("fixtrax_utest")]
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
      i.Add(new CmdLine.Parameter(new[] { "target", "ds" }, "target deployment set", true));
      i.Add(new CmdLine.Parameter(new[] { "branch", "tb" }, "branch of target", false, "PCP/VA30"));
      i.Add(new CmdLine.Parameter(new[] { "workitem", "wi", "bi" }, "workitem to track", false));
      if (!i.Parse(string.Join(" ", args)))
      {
        i.PrintErrors("push.exe");
        return;
      }

      var dsName = i.ValueOf("target");
      var dsBranch = i.ValueOf("branch");

      if (i.IsSpecified("changeset"))
      {
        int csid;
        if (!int.TryParse(i.ValueOf("changeset"), out csid) || csid <= 0)
        {
          Console.WriteLine("The specified changeset id is not valid");
          return;
        }
        tracker.TrackCS(csid, dsName, dsBranch);
        return;
      }

      if (i.IsSpecified("module") || i.IsSpecified("modules"))
      {
        int days;
        if (!int.TryParse(i.ValueOf("days"), out days) || days <= 0)
        {
          Console.WriteLine("The specified number of days is not valid");
          return;
        }
        if (i.IsSpecified("module")) tracker.TrackModuleChanges(i.ValueOf("module"), days, dsName, dsBranch);
        else tracker.TrackModuleChangesFromInputFile(i.ValueOf("modules"), days, dsName, dsBranch);
        return;
      }

      if (i.IsSpecified("workitem"))
      {
        int workitem;
        if (!int.TryParse(i.ValueOf("workitem"), out workitem) || workitem <= 0)
        {
          Console.WriteLine("The specified workitem id is not valid");
          return;
        }
        tracker.TrackWorkitem(workitem, dsName, dsBranch);
        return;
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

    internal void TrackWorkitem(int workitem, string dsName, string dsBranch)
    {
      var wi = WIS.GetWorkItem(workitem);
      Console.WriteLine(wi.ToString());
      foreach (var cs in GetLinkedChangesetsOf(wi).ToList())
      {
        TrackCS(cs.ChangesetId, dsName, dsBranch);
      }
    }

    public void TrackModuleChangesFromInputFile(string moduleFile, int days, string dsName, string dsBranch)
    {
      using (var f = new StreamReader(new FileStream(moduleFile, FileMode.Open, FileAccess.Read)))
      {
        while (!f.EndOfStream)
        {
          var moduleBranch = f.ReadLine();
          TrackModuleChanges(moduleBranch, days, dsName, dsBranch);
        }
      }
    }

    public void TrackModuleChanges(string moduleBranch, int days, string dsName, string dsBranch)
    {
      // load history of the module for the specified number of days
      VCS.QueryHistory(
          new QueryHistoryParameters(string.Join("/", ModulesRootPath, moduleBranch), RecursionType.Full)
          {
            SortAscending = true,
            VersionStart = new DateVersionSpec(DateTime.Today - TimeSpan.FromDays(days))
          })
          .ToList()
          .ForEach(c =>
          {
            if (c.WorkItems.Any()) TrackCS(c.ChangesetId, dsName, dsBranch);
          });
    }

    public void TrackCS(int csid, string dsName, string dsBranch)
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
      Out.Log("{1} CS {0} identified as subsequent version upload", moduleChange.Item.ChangesetId, scpVersionFileName);
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
            string.Join("/", DSRootPath, dsName, dsBranch, VersionInfoFolderPath),
            scpVersionFileName);
        if (modulePush == null)
        {
          PrintTrackingResult(cs, scpName, moduleVersion, dsName, null);
          return;
        }
        var modulVersionInTorus = GetModuleVersion(modulePush.Item);
      }

      // load history of the Torus version file
      // find next change after the module version arrived Torus
      var dsChange = FindVersionOf(
          VCS.GetChangeset(moduleChange.Item.ChangesetId),
          string.Join("/", DSRootPath, dsName, dsBranch, VersionInfoFolderPath),
          string.Format(VersionInfoFilePattern, "Deploy", dsName));
      string dsVersion = null;
      if(dsChange!=null) dsVersion= GetModuleVersion(dsChange.Item);

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
      Console.WriteLine("{0} {1}", dsName, dsVersion);
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
      var changesets = VCS.QueryHistory(string.Join("/", vfolder, vfile), RecursionType.None, 10);
      if (changesets.Count() == 0) throw new FixTraxException("Could not find module version history for \"{0}\"", vfile);
      var i = 1;
      var found = false;
      while (!(found = (changesets.Last().ChangesetId < cs.ChangesetId && i++ < 11)))
      {
        changesets = VCS.QueryHistory(string.Join("/", vfolder, vfile), RecursionType.None, i * 10);
      }
      if (!found) return null;
      var versioncs = changesets.ToList().LastOrDefault(c => c.ChangesetId > cs.ChangesetId);
      if (versioncs == null) return null;
      return versioncs.Changes.SingleOrDefault(c => c.Item.ServerItem.EndsWith(vfile, StringComparison.InvariantCultureIgnoreCase));
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

    internal IEnumerable<IChangeset> GetLinkedChangesetsOf(IWorkItem wi)
    {
      var result = new List<ChangesetWrapper>();
      foreach (Link link in wi.Links)
      {
        ExternalLink extLink = link as ExternalLink;
        if (extLink != null)
        {
          ArtifactId artifact = LinkingUtilities.DecodeUri(extLink.LinkedArtifactUri);
          if (String.Equals(artifact.ArtifactType, "Changeset", StringComparison.Ordinal))
          {
            // Convert the artifact URI to Changeset object.
            result.Add(new ChangesetWrapper(VCS.ArtifactProvider.GetChangeset(new Uri(extLink.LinkedArtifactUri))));
          }
        }
      }
      return result;
    }

    private IVersionControlServer vcs;
    internal IVersionControlServer VCS
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

  public static class Out
  {
    public static int VerbosityLevel = 0;
    public static void Log(string fmt, params object[] args) { if (VerbosityLevel > 0) Print(ConsoleColor.DarkGray, fmt, args); }

    private static void Print(ConsoleColor color, string fmt, params object[] args)
    {
      Console.ForegroundColor = color;
      Console.WriteLine(fmt, args);
      Console.ResetColor();
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
