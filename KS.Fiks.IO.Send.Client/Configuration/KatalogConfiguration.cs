namespace KS.Fiks.IO.Send.Client.Configuration
{
    public class KatalogConfiguration : ApiConfiguration
    {
        private const string DefaultPath = "/fiks-io/katalog/api/v1";

        private const string DefaultKontoPath = "/fiks-io/konto/api/v1";

        public KatalogConfiguration(string path = null, string scheme = null, string host = null, int? port = null)
            : this(path, scheme, host, port, null)
        {
        }

        public KatalogConfiguration(string path, string scheme, string host, int? port, string kontoPath)
            : base(scheme, host, port)
        {
            Path = path ?? DefaultPath;
            KontoPath = kontoPath ?? DefaultKontoPath;
        }

        public KatalogConfiguration(ApiConfiguration apiConfiguration)
            : base(apiConfiguration)
        {
            Path = DefaultPath;
            KontoPath = DefaultKontoPath;
        }

        public string Path { get; }

        /// <summary>
        /// Path to the Fiks-IO konto API, used for uploading public keys. The catalog API is read-only.
        /// </summary>
        public string KontoPath { get; }
    }
}
