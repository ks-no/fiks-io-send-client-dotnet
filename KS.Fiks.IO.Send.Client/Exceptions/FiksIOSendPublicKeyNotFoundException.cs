using System;

namespace KS.Fiks.IO.Send.Client.Exceptions
{
    /// <summary>
    /// Thrown by <see cref="Catalog.ICatalogHandler.GetPublicKey"/> when the catalog has no public key
    /// registered for the requested account (HTTP 404 or an empty key payload). Distinguishes a genuinely
    /// missing key from a transient catalog failure. Inherits from
    /// <see cref="FiksIOSendUnexpectedResponseException"/> so existing catch blocks keep working.
    /// </summary>
    public class FiksIOSendPublicKeyNotFoundException : FiksIOSendUnexpectedResponseException
    {
        public FiksIOSendPublicKeyNotFoundException()
        {
        }

        public FiksIOSendPublicKeyNotFoundException(string message)
            : base(message)
        {
        }

        public FiksIOSendPublicKeyNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
