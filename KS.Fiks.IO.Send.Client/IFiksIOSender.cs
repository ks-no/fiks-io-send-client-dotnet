using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Crypto.Models;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Send.Client
{
    public interface IFiksIOSender
    {
        Task<SendtMeldingApiModel> SendWithEncryptedData(MeldingSpesifikasjonApiModel metaData, IList<IPayload> payload);

        Task<SendtMeldingApiModel> Send(MeldingSpesifikasjonApiModel metaData, Stream data);

        Task<SendtMeldingApiModel> Send(MeldingSpesifikasjonApiModel metaData);
    }
}