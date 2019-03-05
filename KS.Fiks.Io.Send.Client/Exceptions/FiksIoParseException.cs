using System;

namespace KS.Fiks.Io.Send.Client.Exceptions
{
    public class FiksIoParseException : Exception
    {
        public FiksIoParseException(string message)
            : base(message)
        {
        }

        public FiksIoParseException()
        {
        }

        public FiksIoParseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}