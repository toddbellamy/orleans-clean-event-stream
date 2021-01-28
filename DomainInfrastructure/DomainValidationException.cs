using System;


namespace DomainBase
{
    public class DomainValidationException : Exception
    {
        public DomainValidationException(string message)
            :base(message)
        { }
    }
}
