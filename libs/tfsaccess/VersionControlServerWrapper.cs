using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace tfsaccess
{
    public interface IVersionControlServer
    {
        IChangeset GetChangeset(int id);
        IItem GetItem(string path);
        IItemSet GetItems(string path);
        IEnumerable<IChangeset> QueryHistory(string item, RecursionType recursion, int maxResults);
        IEnumerable<IChangeset> QueryHistory(QueryHistoryParameters qhp);
        VersionControlArtifactProvider ArtifactProvider { get; }
    }

    public class VersionControlServerWrapper : IVersionControlServer
    {
        private VersionControlServer vcs;
        private VersionControlServer VCS
        {
            get
            {
                if (vcs == null) throw new TFSAccessException("VersionControlServer object is not initialized.");
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

        public IEnumerable<IChangeset> QueryHistory(string item, RecursionType recursion, int maxResults)
        {
            var qhp = new QueryHistoryParameters(item, recursion)
            {
                MaxResults = maxResults,
                IncludeChanges = true
            };
            return VCS.QueryHistory(qhp).Select(c => new ChangesetWrapper(c));
        }

        public IEnumerable<IChangeset> QueryHistory(QueryHistoryParameters qhp)
        {
            return VCS.QueryHistory(qhp).Select(c => new ChangesetWrapper(c));
        }

        public VersionControlArtifactProvider ArtifactProvider { get { return VCS.ArtifactProvider; } }

    }

}
