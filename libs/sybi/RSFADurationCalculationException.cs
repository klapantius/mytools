using System;

namespace sybi
{
    public class RSFADurationCalculationException : Exception
    {
        public RSFADurationCalculationException(string format, params object[] args)
            :base(string.Format(format, args))
        {
        }
    }
}
