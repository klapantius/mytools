using System;

using juba.tfs.interfaces;


namespace sybi
{
    public interface IVersionInfo
    {
        string Id { get; }
        IItem Item { get; }
        string VersionFolder { get; }
        string VersionFile { get; }
    }
}
