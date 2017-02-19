using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
