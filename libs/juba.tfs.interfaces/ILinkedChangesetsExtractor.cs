using System.Collections.Generic;


namespace juba.tfs.interfaces
{
    public interface ILinkedChangesetsExtractor
    {
        IEnumerable<IChangeset> GetChangesets(IWorkItem wi);
    }
}