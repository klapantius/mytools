using System;

namespace juba.tfs.wrappers
{
    public class TfsAccessException : Exception
    {
        public TfsAccessException(string fmt, params object[] args)
            : base(string.Format(fmt, args))
        {
        }
    }
}
