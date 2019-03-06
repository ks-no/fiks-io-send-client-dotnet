using System;

namespace KS.Fiks.Io.Send.Client.Exceptions
{
    public class FiksIoSendUnexpectedResponseException : Exception
    {
        public FiksIoSendUnexpectedResponseException()
        {
        }

        public FiksIoSendUnexpectedResponseException(string message)
            : base(message)
        {
        }

        public FiksIoSendUnexpectedResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}