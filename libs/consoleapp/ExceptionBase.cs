using System;

namespace juba.consoleapp
{
    public class ExceptionBase : Exception
    {
        public ExceptionBase(string fmt, params object[] args)
            : base(string.Format(fmt, args))
        {

        }
    }
}
