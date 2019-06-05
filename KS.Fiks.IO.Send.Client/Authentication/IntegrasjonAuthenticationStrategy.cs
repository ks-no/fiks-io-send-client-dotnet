using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ks.Fiks.Maskinporten.Client;

namespace KS.Fiks.IO.Send.Client.Authentication
{
    public class IntegrasjonAuthenticationStrategy : IAuthenticationStrategy
    {
        private const string DefaultScope = "ks";

        private readonly IMaskinportenClient _maskinportenClient;
        private readonly Guid _integrasjonId;
        private readonly string _integrasjonPassord;

        public IntegrasjonAuthenticationStrategy(
            IMaskinportenClient maskinportenClient,
            Guid integrasjonId,
            string integrasjonPassord)
        {
            _maskinportenClient = maskinportenClient;
            _integrasjonId = integrasjonId;
            _integrasjonPassord = integrasjonPassord;
        }

        public async Task<Dictionary<string, string>> GetAuthorizationHeaders()
        {
            var auth = await _maskinportenClient.GetAccessToken(DefaultScope).ConfigureAwait(false);
            return new Dictionary<string, string>
            {
                { "AUTHORIZATION", $"Bearer {auth.Token}" },
                { "IntegrasjonId", _integrasjonId.ToString() },
                { "IntegrasjonPassord", _integrasjonPassord }
            };
        }
    }
}