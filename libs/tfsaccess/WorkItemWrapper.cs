using juba.tfs.interfaces;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;

namespace juba.tfs.wrappers
{
    public class WorkItemWrapper : IExtendedWorkItem
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

        public IEnumerable<IChangeset> LinkedChangesets(IExtendedVersionControlServer vcs)
        {
            var result = new List<IChangeset>();
            foreach (Link link in wi.Links)
            {
                ExternalLink extLink = link as ExternalLink;
                if (extLink != null)
                {
                    ArtifactId artifact = LinkingUtilities.DecodeUri(extLink.LinkedArtifactUri);
                    if (String.Equals(artifact.ArtifactType, "Changeset", StringComparison.Ordinal))
                    {
                        // Convert the artifact URI to Changeset object.
                        result.Add(vcs.ArtifactProviderGetChangeset(new Uri(extLink.LinkedArtifactUri)));
                    }
                }
            }
            return result;
        }

    }
}
