using System;

namespace sybi
{
    interface IVersionInfo
    {
        string ScpName { get; }
        string Version { get; }
    }
}
