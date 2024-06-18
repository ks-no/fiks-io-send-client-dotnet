using System;

namespace KS.Fiks.IO.Send.Client.Exceptions
{
    public class FiksIOSendEncryptionException : Exception
    {
        public FiksIOSendEncryptionException(string message)
            : base(message)
        {
        }

        public FiksIOSendEncryptionException()
        {
        }

        public FiksIOSendEncryptionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}