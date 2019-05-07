using System.IO;
using System.Threading.Tasks;
using KS.Fiks.IO.Send.Client.Models;

namespace KS.Fiks.IO.Send.Client
{
    public interface IFiksIOSender
    {
        Task<SentMessageApiModel> Send(MessageSpecificationApiModel metaData, Stream data);
    }
}