using juba.tfs.interfaces;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace juba.tfs.wrappers
{
    public class VersionControlServerWrapper : IExtendedVersionControlServer
    {
        private VersionControlServer vcs;
        private VersionControlServer VCS
        {
            get
            {
                if (vcs == null) throw new TfsAccessException("VersionControlServer object is not initialized.");
                return vcs;
            }
        }
        public VersionControlServerWrapper(Uri serverUri)
        {
            var server = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(serverUri);
            vcs = server.GetService<VersionControlServer>();
        }


        public IChangeset GetChangeset(int id)
        {
            return new ChangesetWrapper(VCS.GetChangeset(id));
        }

        public IItem GetItem(string path)
        {
            return new ItemWrapper(VCS.GetItem(path, VersionSpec.Latest, DeletedState.Any, GetItemsOptions.IncludeBranchInfo));
        }

        public IItemSet GetItems(string path)
        {
            return new ItemSetWrapper(VCS.GetItems(new ItemSpec(path, RecursionType.None), VersionSpec.Latest, DeletedState.NonDeleted, ItemType.Any, GetItemsOptions.IncludeBranchInfo));
        }

        public IChangeset ArtifactProviderGetChangeset(Uri artifactUri) { return new ChangesetWrapper(VCS.ArtifactProvider.GetChangeset(artifactUri)); }

        public IEnumerable<IChangeset> QueryHistory(string itemSpec, bool fullRecursion, bool sortAscending, DateTime dateVersionStart, bool includeChanges)
        {
            var qhp = new QueryHistoryParameters(itemSpec, fullRecursion ? RecursionType.Full : RecursionType.None)
            {
                SortAscending = sortAscending,
                IncludeChanges = includeChanges,
                VersionStart = new DateVersionSpec(dateVersionStart)
            };
            return VCS.QueryHistory(qhp).Select(c => new ChangesetWrapper(c));
        }

        public IEnumerable<IChangeset> QueryHistory(string itemSpec, bool fullRecursion, bool sortAscending, IChangeset changeSet, bool includeChanges)
        {
            var qhp = new QueryHistoryParameters(itemSpec, fullRecursion ? RecursionType.Full : RecursionType.None)
            {
                SortAscending = sortAscending,
                IncludeChanges = includeChanges,
                VersionStart = new ChangesetVersionSpec(changeSet.ChangesetId)
            };
            return VCS.QueryHistory(qhp).Select(c => new ChangesetWrapper(c));
        }
    }

}
