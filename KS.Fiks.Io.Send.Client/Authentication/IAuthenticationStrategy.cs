using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace KS.Fiks.IO.Send.Client
{
    public interface IAuthenticationStrategy
    {
        Task<Dictionary<string, string>> GetAuthorizationHeaders();
    }
}