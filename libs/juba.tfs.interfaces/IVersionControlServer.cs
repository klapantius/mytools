using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;

namespace juba.tfs.interfaces
{
    public interface IVersionControlServer
    {
        IChangeset GetChangeset(int id);
        IItem GetItem(string path);
        IItemSet GetItems(string path);
        IEnumerable<IChangeset> QueryHistory(string itemSpec, bool fullRecursion, bool sortAscending, DateTime dateVersionStart, bool includeChanges);
        IEnumerable<IChangeset> QueryHistory(string itemSpec, bool fullRecursion, bool sortAscending, IChangeset changeSet, bool includeChanges);
        Shelveset[] QueryShelvesets(string shelvesetName, string shelvesetOwner);
        PendingSet[] QueryShelvedChanges(string shelvesetName, string shelvesetOwner);
    }

}
