using juba.tfs.interfaces;

using Microsoft.TeamFoundation.TestManagement.Client;


namespace juba.tfs.wrappers
{
    public class TestManagementTeamProjectProvider : ITestManagementTeamProjectProvider
    {
        private readonly ITfsTeamProjectCollection myTfsTeamProjectCollection;

        public TestManagementTeamProjectProvider(ITfsTeamProjectCollection tfsTeamProjectCollection)
        {
            myTfsTeamProjectCollection = tfsTeamProjectCollection;
        }

        public ITestManagementTeamProject GeTestManagementTeamProject(string teamProject)
        {
            var svc = myTfsTeamProjectCollection.GetService<ITestManagementService>();
            return svc.GetTeamProject(teamProject);
        }
    }
}
