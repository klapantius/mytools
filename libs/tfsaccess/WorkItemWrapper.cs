using juba.tfs.interfaces;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace juba.tfs.wrappers
{
    public class WorkItemWrapper : IWorkItem
    {
        private WorkItem wi;
        private WorkItem WI
        {
            get
            {
                if (wi == null) throw new TfsAccessException("Not initialized object of type Workitem.");
                return wi;
            }
            set { wi = value; }
        }
        public WorkItemWrapper(WorkItem workitem)
        {
            WI = workitem;
        }

        public int Id { get { return WI.Id; } }

        public override string ToString()
        {
            return string.Format("WI {0} {1} \"{2}\" ", WI.Id, WI.State, WI.Title);
        }

    }
}
