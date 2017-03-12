using System.Collections.Generic;


namespace juba.tfs.interfaces
{
    public class LinkedChangesetsExtractor : ILinkedChangesetsExtractor
    {
        private readonly IVersionControlServer myVcs;

        public LinkedChangesetsExtractor(IVersionControlServer vcs)
        {
            myVcs = vcs;
        }

        public IEnumerable<IChangeset> GetChangesets(IWorkItem wi)
        {
            throw new System.NotImplementedException();
        }

    }
}