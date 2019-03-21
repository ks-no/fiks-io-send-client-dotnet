using System;

namespace KS.Fiks.IO.Send.Client.Exceptions
{
    public class FiksIOSendUnexpectedResponseException : Exception
    {
        public FiksIOSendUnexpectedResponseException()
        {
        }

        public FiksIOSendUnexpectedResponseException(string message)
            : base(message)
        {
        }

        public FiksIOSendUnexpectedResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}