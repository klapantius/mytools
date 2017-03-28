using Microsoft.TeamFoundation.TestManagement.Client;


namespace juba.tfs.interfaces
{
    public interface ITestManagementTeamProjectProvider
    {
        ITestManagementTeamProject GeTestManagementTeamProject(string teamProject);
    }
}
