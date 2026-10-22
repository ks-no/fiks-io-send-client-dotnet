using System;
using System.Threading;
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
        /// Uploads the X.509 certificate containing the account's public key to the Fiks-IO konto API.
        /// </summary>
        /// <param name="kontoId">The account ID the certificate belongs to.</param>
        /// <param name="pemString">The PEM-encoded X.509 certificate meant for encryption.</param>
        /// <param name="cancellationToken">Token for cancelling the upload.</param>
        Task UploadPublicKey(Guid kontoId, string pemString, CancellationToken cancellationToken = default);
    }
}