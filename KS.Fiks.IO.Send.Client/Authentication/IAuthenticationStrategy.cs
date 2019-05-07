using System.Collections.Generic;
using System.Threading.Tasks;

namespace KS.Fiks.IO.Send.Client.Authentication
{
    public interface IAuthenticationStrategy
    {
        Task<Dictionary<string, string>> GetAuthorizationHeaders();
    }
}