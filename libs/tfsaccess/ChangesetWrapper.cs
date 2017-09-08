using juba.tfs.interfaces;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Linq;

namespace juba.tfs.wrappers
{
    public class ChangesetWrapper : IChangeset
    {
        private Changeset cs;
        private Changeset CS
        {
            get
            {
                if (cs == null) throw new TfsAccessException("Not initialized object of type Changeset.");
                return cs;
            }
            set { cs = value; }
        }
        public ChangesetWrapper(Changeset changeset)
        {
            CS = changeset;
        }

        public IChange[] Changes
        {
            get
            {
                return CS.Changes.Select(c => new ChangeWrapper(c)).ToArray();
            }
        }

        public string Comment { get { return CS.Comment; }}

        public DateTime CreationDate { get { return CS.CreationDate; } }
        public int ChangesetId { get { return CS.ChangesetId; } }
        public IWorkItem[] WorkItems { get { return CS.WorkItems.Select(wi => new WorkItemWrapper(wi)).ToArray(); } }

        public override string ToString()
        {
            return string.Format("CS {0} created {1} by {2} ({3}): {4}", CS.ChangesetId, CS.CreationDate, CS.OwnerDisplayName, CS.Owner, CS.Comment.Replace(Environment.NewLine, ""));
        }
        
    }
}
