using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using juba.tfs.interfaces;


namespace sybi
{
    public interface IBranchFinder
    {
        IBranch Find(string path);
        IBranch Find(IItem item);
    }
}
