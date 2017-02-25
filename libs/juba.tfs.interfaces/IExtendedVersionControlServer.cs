using System;

namespace juba.tfs.interfaces
{
    public interface IExtendedVersionControlServer : IVersionControlServer
    {
        IChangeset ArtifactProviderGetChangeset(Uri artifactUri);
    }
}
