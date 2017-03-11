using System;


namespace sybi
{
    public class SybiException : Exception
    {
        public SybiException(string fmt, params object[] args)
            : base(string.Format(fmt, args))
        {

        }

        public SybiException(Exception innerException, string fmt, params object[] args)
            : base(string.Format(fmt, args), innerException)
        {

        }
    }
}
