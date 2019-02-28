using System.IO;
using System.Threading.Tasks;

namespace KS.Fiks.Io.Send.Client
{
    public interface IFiksIOSender
    {
        Task<SentMessageApiModel> Send(MessageSpecificationApiModel metaData, Stream data);
    }
}