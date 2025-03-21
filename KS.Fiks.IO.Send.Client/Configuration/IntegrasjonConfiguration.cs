using System;

namespace KS.Fiks.IO.Send.Client.Configuration
{
    public class IntegrasjonConfiguration
    {
        private const string DefaultScope = "ks:fiks";

        public IntegrasjonConfiguration(Guid integrasjonId, string integrasjonPassord, string scope = null)
        {
            IntegrasjonId = integrasjonId;
            IntegrasjonPassord = integrasjonPassord.Trim();
            Scope = scope ?? DefaultScope;
        }

        public Guid IntegrasjonId { get; }

        public string IntegrasjonPassord { get; }

        public string Scope { get; }
    }
}