using System;
using System.Threading.Tasks;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Send.Client.Catalog
{
    public class CatalogPublicKeyProvider : IPublicKeyProvider
    {
        private readonly ICatalogHandler _catalogHandler;

        public CatalogPublicKeyProvider(ICatalogHandler catalogHandler)
        {
            _catalogHandler = catalogHandler;
        }

        public Task<X509Certificate> GetPublicKey(Guid receiverAccountId)
        {
            return _catalogHandler.GetPublicKey(receiverAccountId);
        }
    }
}