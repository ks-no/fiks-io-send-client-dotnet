using KS.Fiks.IO.Encryption.Configuration;

namespace KS.Fiks.IO.Send.Client.Configuration
{
    public class FiksIOSenderConfiguration
    {
        private const string DefaultPath = "/fiks-io/api/v1/send";

        private const string DefaultScheme = "https";

        private const string DefaultHost = "api.fiks.ks.no";

        private const int DefaultPort = 443;

        public AsiceSigningConfiguration AsiceSigningConfiguration { get; }

        public IntegrasjonConfiguration IntegrasjonConfiguration { get; }

        public FiksIOSenderConfiguration(
            string path = null,
            string scheme = null,
            string host = null,
            int? port = null,
            AsiceSigningConfiguration asiceSigningConfiguration = null,
            IntegrasjonConfiguration integrasjonConfiguration = null)
        {
            Path = path ?? DefaultPath;
            Scheme = scheme ?? DefaultScheme;
            Host = host ?? DefaultHost;
            Port = port ?? DefaultPort;
            AsiceSigningConfiguration = asiceSigningConfiguration;
            IntegrasjonConfiguration = integrasjonConfiguration;
        }

        public string Path { get; }

        public string Scheme { get; }

        public string Host { get; }

        public int Port { get; }
    }
}