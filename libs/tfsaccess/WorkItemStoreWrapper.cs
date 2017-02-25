using juba.tfs.interfaces;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;

namespace juba.tfs.wrappers
{
    public class WorkItemStoreWrapper : IWorkItemStore
    {
        private WorkItemStore wis;
        private WorkItemStore WIS
        {
            get
            {
                if (wis == null) throw new TfsAccessException("WorkItemStore object is not initialized.");
                return wis;
            }
        }
        public WorkItemStoreWrapper(Uri serverUri)
        {
            var server = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(serverUri);
            wis = server.GetService<WorkItemStore>();
        }

        public IExtendedWorkItem GetWorkItem(int id) { return new WorkItemWrapper(WIS.GetWorkItem(id)); }

    }
}
