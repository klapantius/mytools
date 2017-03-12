using System;
using System.Linq;

using juba.tfs.interfaces;


namespace sybi
{
    public class VersionInfoFinder : IVersionInfoFinder
    {
        private readonly IVersionControlServer myVCS;

        public VersionInfoFinder(IVersionControlServer vcs)
        {
            myVCS = vcs;
        }

        public IVersionInfo FindByChangeset(IChangeset cs, string vfolder, string vfile)
        {
            if (cs == null) return null;

            // ask younger changesets of the specified version information file
            var changesets =
                myVCS.QueryHistory(string.Join("/", vfolder, vfile), fullRecursion: false, sortAscending: true, changeSet: cs, includeChanges: true).ToList();
            // take the first one after the specified changeset
            var versioncs = changesets.FirstOrDefault(c => c.ChangesetId > cs.ChangesetId);
            if (versioncs == null)
            {
                return new VersionInfo();
            }
            var change = versioncs.Changes.SingleOrDefault(c => c.Item.ServerItem.EndsWith(vfile, StringComparison.InvariantCultureIgnoreCase));
            // so, there should be a change of the asked file in this changeset, but who knows...
            if (change != null) return new VersionInfo(change.Item);
            return new VersionInfo();
        }

        public IVersionInfo FindByChangesetId(int cs, string vfolder, string vfile)
        {
            return FindByChangeset(myVCS.GetChangeset(cs), vfolder, vfile);
        }

        public IVersionInfo FindByChangeset(IChangeset cs)
        {
            if (cs == null) return null;

            var anItemOfCS = cs.Changes.First().Item.ServerItem;
            var scpFinder = new SourceControlProjectFinder();
            var scp = scpFinder.Find(anItemOfCS);
            var brFinder = new BranchFinder(myVCS);
            var scpVersionFolder = string.Join("/", brFinder.Find(anItemOfCS).Path, PathAndFilenameConventions.VersionInfoFolderSubpath);
            var scpType = scpVersionFolder.Split('/')[2];
            var scpVersionFileName = string.Format(PathAndFilenameConventions.VersionInfoFilePattern, scpType, scp.Name);

            return FindByChangeset(cs, scpVersionFolder, scpVersionFileName);
        }

        public IVersionInfo FindByChangesetId(int cs)
        {
            return FindByChangeset(myVCS.GetChangeset(cs));
        }

    }
}
