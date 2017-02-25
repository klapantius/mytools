using System.Collections.Generic;

namespace sybi
{
    public interface ISourceControlProject
    {
        IEnumerable<IBranch> Branches { get; }
    }
    public class SourceControlProject
    {
    }
}
