using System;

namespace sybi
{
    public interface IVersionInfo
    {
        string ScpName { get; }
        string Version { get; }
    }
}
