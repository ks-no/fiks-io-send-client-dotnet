using System;

namespace KS.Fiks.IO.Send.Client.Exceptions
{
    public class FiksIOSendUnauthorizedException : Exception
    {
        public FiksIOSendUnauthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public FiksIOSendUnauthorizedException(string message)
            : base(message)
        {
        }

        public FiksIOSendUnauthorizedException()
        {
        }
    }
}