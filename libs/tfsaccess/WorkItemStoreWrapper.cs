using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tfsaccess
{
    public interface IWorkItemStore
    {
        IWorkItem GetWorkItem(int id);
    }

    public class WorkItemStoreWrapper : IWorkItemStore
    {
        private WorkItemStore wis;
        private WorkItemStore WIS
        {
            get
            {
                if (wis == null) throw new TFSAccessException("WorkItemStore object is not initialized.");
                return wis;
            }
        }
        public WorkItemStoreWrapper(Uri serverUri)
        {
            var server = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(serverUri);
            wis = server.GetService<WorkItemStore>();
        }

        public IWorkItem GetWorkItem(int id) { return new WorkItemWrapper(WIS.GetWorkItem(id)); }

    }
}
