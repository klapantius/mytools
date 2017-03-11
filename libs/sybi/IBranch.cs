using System.Collections.Generic;


namespace sybi
{
    public interface IBranch
    {
        string Path { get; }
        IEnumerable<IVersionInfo> LatestVersionInfo { get; }
    }
}