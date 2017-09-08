using System;

namespace juba.tfs.interfaces
{
    public interface IChangeset
    {
        IChange[] Changes { get; }
        DateTime CreationDate { get; }
        int ChangesetId { get; }
        IWorkItem[] WorkItems { get; }
        string Comment { get; }
    }

}
