using System.Collections.Generic;

namespace juba.tfs.interfaces
{
    public interface IExtendedWorkItem : IWorkItem
    {
        IEnumerable<IChangeset> LinkedChangesets(IExtendedVersionControlServer vcs);
    }
}
