using System;

namespace KS.Fiks.Io.Send.Client.Exceptions
{
    public class FiksIoSendParseException : Exception
    {
        public FiksIoSendParseException(string message)
            : base(message)
        {
        }

        public FiksIoSendParseException()
        {
        }

        public FiksIoSendParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}