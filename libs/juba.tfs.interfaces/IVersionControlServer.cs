using Microsoft.TeamFoundation.VersionControl.Client;
using System.Collections.Generic;

namespace juba.tfs.interfaces
{
    public interface IVersionControlServer
    {
        IChangeset GetChangeset(int id);
        IItem GetItem(string path);
        IItemSet GetItems(string path);
        IEnumerable<IChangeset> QueryHistory(string item, RecursionType recursion, int maxResults);
        IEnumerable<IChangeset> QueryHistory(QueryHistoryParameters qhp);
    }

}
