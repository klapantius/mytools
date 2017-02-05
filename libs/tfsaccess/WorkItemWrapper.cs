using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace tfsaccess
{
    public interface IWorkItem
    {
        int Id { get; }
        LinkCollection Links { get; }
    }
    public class WorkItemWrapper : IWorkItem
    {
        private WorkItem wi;
        private WorkItem WI
        {
            get
            {
                if (wi == null) throw new TFSAccessException("Not initialized object of type Workitem.");
                return wi;
            }
            set { wi = value; }
        }
        public WorkItemWrapper(WorkItem workitem)
        {
            WI = workitem;
        }

        public int Id { get { return WI.Id; } }
        public LinkCollection Links { get { return WI.Links; } }

        public override string ToString()
        {
            return string.Format("WI {0} {1} \"{2}\" ", WI.Id, WI.State, WI.Title, WI.Links);
        }
    }
}
