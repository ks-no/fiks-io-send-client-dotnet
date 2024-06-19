using System;
using System.Security.Cryptography.X509Certificates;
using KS.Fiks.IO.Crypto.Configuration;

namespace KS.Fiks.IO.Send.Client.Configuration
{
    public class FiksIOSenderConfigurationBuilder
    {
        private FiksIOSenderConfiguration _configuration;
        private AsiceSigningConfiguration _asiceSigningConfiguration;
        private IntegrasjonConfiguration _integrasjonConfiguration;

        private string _path;
        private string _scheme;
        private string _host;
        private int? _port;

        // private string _maskinportenIssuer = string.Empty;
        // private X509Certificate2 _maskinportenCertificate;

        public FiksIOSenderConfiguration Build()
        {
            _configuration = new FiksIOSenderConfiguration(_path, _scheme, _host, _port, _asiceSigningConfiguration, _integrasjonConfiguration);
            return _configuration;
        }

        // public FiksIOSenderConfigurationBuilder WithMaskinportenConfiguration(X509Certificate2 certificate, string issuer)
        // {
        //     _maskinportenIssuer = issuer;
        //     _maskinportenCertificate = certificate;
        //     return this;
        // }

        public FiksIOSenderConfigurationBuilder WithAsiceSigningConfiguration(string publicKeyPath, string privateKeyPath)
        {
            _asiceSigningConfiguration = new AsiceSigningConfiguration(publicKeyPath, privateKeyPath);
            return this;
        }

        public FiksIOSenderConfigurationBuilder WithAsiceSigningConfiguration(X509Certificate2 x509Certificate2)
        {
            _asiceSigningConfiguration = new AsiceSigningConfiguration(x509Certificate2);
            return this;
        }

        public FiksIOSenderConfigurationBuilder WithFiksIntegrasjonConfiguration(Guid fiksIntegrasjonId, string fiksIntegrasjonPassword)
        {
            _integrasjonConfiguration = new IntegrasjonConfiguration(fiksIntegrasjonId, fiksIntegrasjonPassword);
            return this;
        }

        public FiksIOSenderConfigurationBuilder WithApiConfiguration(string path = null, string scheme = null, string host = null, int? hostPort = null)
        {
            _path = path;
            _scheme = scheme;
            _host = host;
            _port = hostPort;
            return this;
        }
    }
}