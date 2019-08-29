using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Send.Client
{
    public interface IFiksIOSender
    {
        Task<SendtMeldingApiModel> Send(MeldingSpesifikasjonApiModel metaData, Stream data);

        Task<SendtMeldingApiModel> Send(MeldingSpesifikasjonApiModel metaData);

    }
}