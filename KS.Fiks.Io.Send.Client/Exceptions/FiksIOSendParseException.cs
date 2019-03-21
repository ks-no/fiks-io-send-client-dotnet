using System;

namespace KS.Fiks.IO.Send.Client.Exceptions
{
    public class FiksIOSendParseException : Exception
    {
        public FiksIOSendParseException(string message)
            : base(message)
        {
        }

        public FiksIOSendParseException()
        {
        }

        public FiksIOSendParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}