using System;
using System.Threading.Tasks;
using KS.Fiks.IO.Send.Client.Models;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Send.Client.Catalog
{
    public interface ICatalogHandler
    {
        Task<Konto> Lookup(LookupRequest request);

        Task<Konto> GetKonto(Guid kontoId);

        Task<X509Certificate> GetPublicKey(Guid receiverAccountId);

        Task<Status> GetStatus(Guid receiverAccountId);

        /// <summary>
        /// Uploads the public key (PEM) for the given integration account to the Fiks catalog.
        /// </summary>
        /// <param name="kontoId">The account ID the key belongs to.</param>
        /// <param name="pemString">The PEM-encoded public key to upload.</param>
        Task UploadPublicKey(Guid kontoId, string pemString);
    }
}