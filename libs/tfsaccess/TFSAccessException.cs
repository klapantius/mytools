using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tfsaccess
{
    public class TFSAccessException : Exception
    {
        public TFSAccessException(string fmt, params object[] args)
            : base(string.Format(fmt, args))
        {
        }
    }
}
