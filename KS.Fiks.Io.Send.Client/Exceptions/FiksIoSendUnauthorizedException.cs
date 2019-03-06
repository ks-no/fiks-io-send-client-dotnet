using System;

namespace KS.Fiks.Io.Send.Client.Exceptions
{
    public class FiksIoSendUnauthorizedException : Exception
    {
        public FiksIoSendUnauthorizedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public FiksIoSendUnauthorizedException(string message)
            : base(message)
        {
        }

        public FiksIoSendUnauthorizedException()
        {
        }
    }
}