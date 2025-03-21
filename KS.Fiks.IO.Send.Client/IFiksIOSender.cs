using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Crypto.Models;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Send.Client
{
    public interface IFiksIOSender
    {
        Task<SendtMeldingApiModel> SendWithEncryptedData(MeldingSpesifikasjonApiModel metaData, IPayload payload, CancellationToken cancellationToken = default);

        Task<SendtMeldingApiModel> SendWithEncryptedData(MeldingSpesifikasjonApiModel metaData, IList<IPayload> payload, CancellationToken cancellationToken = default);

        Task<SendtMeldingApiModel> Send(MeldingSpesifikasjonApiModel metaData, Stream data, CancellationToken cancellationToken = default);

        Task<SendtMeldingApiModel> Send(MeldingSpesifikasjonApiModel metaData, CancellationToken cancellationToken = default);
    }
}