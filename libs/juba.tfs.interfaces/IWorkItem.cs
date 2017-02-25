using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace juba.tfs.interfaces
{
    public interface IWorkItem
    {
        int Id { get; }
        LinkCollection Links { get; }
    }

}
