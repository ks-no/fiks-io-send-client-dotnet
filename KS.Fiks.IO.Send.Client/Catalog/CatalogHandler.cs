using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using KS.Fiks.Crypto.BouncyCastle;
using KS.Fiks.IO.Send.Client.Configuration;
using KS.Fiks.IO.Send.Client.Exceptions;
using KS.Fiks.IO.Send.Client.Models;
using Ks.Fiks.Maskinporten.Client;
using Newtonsoft.Json;
using Org.BouncyCastle.X509;

namespace KS.Fiks.IO.Send.Client.Catalog
{
    public class CatalogHandler : ICatalogHandler
    {
        private const string LookupEndpoint = "lookup";

        private const string PublicKeyEndpoint = "offentligNokkel";

        private const string AccountsEndpoint = "kontoer";

        private const string StatusEndpoint = "status";

        private const string IdentifyerQueryName = "identifikator";

        private const string MessageProtocolQueryName = "meldingProtokoll";

        private const string AccessLevelQueryName = "sikkerhetsniva";

        private readonly HttpClient _httpClient;

        private readonly KatalogConfiguration _katalogConfiguration;
        private readonly IntegrasjonConfiguration _integrasjonConfiguration;
        private readonly IMaskinportenClient _maskinportenClient;

        public CatalogHandler(
            KatalogConfiguration katalogConfiguration,
            IntegrasjonConfiguration integrasjonConfiguration,
            IMaskinportenClient maskinportenClient,
            HttpClient httpClient = null)
        {
            _katalogConfiguration = katalogConfiguration;
            _integrasjonConfiguration = integrasjonConfiguration;
            _maskinportenClient = maskinportenClient;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<Konto> Lookup(LookupRequest request)
        {
            var requestUri = CreateLookupUri(request);
            var responseAsAccount = await GetAsModel<KatalogKonto>(requestUri).ConfigureAwait(false);
            return Konto.FromKatalogModel(responseAsAccount);
        }

        public async Task<Konto> GetKonto(Guid kontoId)
        {
            var requestUri = CreateGetKontoUri(kontoId);
            var responseAsAccount = await GetAsModel<KatalogKonto>(requestUri).ConfigureAwait(false);
            return Konto.FromKatalogModel(responseAsAccount);
        }

        public async Task<Status> GetStatus(Guid kontoId)
        {
            var requestUri = CreateGetKontoStatusUri(kontoId);
            var responseAsAccount = await GetAsModel<KontoSvarStatus>(requestUri).ConfigureAwait(false);
            return Status.FromKontoSvarStatusModel(responseAsAccount);
        }

        public async Task<X509Certificate> GetPublicKey(Guid receiverAccountId)
        {
            var requestUri = CreatePublicKeyUri(receiverAccountId);

            // GetPublicKey is unauthenticated, so the request is issued directly here (instead of via
            // GetAsModel) to special-case HTTP 404 / empty payload as "no key registered" without changing
            // the shared response handling used by the other catalog endpoints.
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new FiksIOSendPublicKeyNotFoundException(
                        $"No public key is registered in the catalog for account {receiverAccountId}.");
                }

                await ThrowIfResponseIsInvalid(response, requestUri).ConfigureAwait(false);

                var responseAsJsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var responseAsPublicKeyModel = JsonConvert.DeserializeObject<KontoOffentligNokkel>(responseAsJsonString);

                if (string.IsNullOrWhiteSpace(responseAsPublicKeyModel?.Nokkel))
                {
                    throw new FiksIOSendPublicKeyNotFoundException(
                        $"No public key is registered in the catalog for account {receiverAccountId}.");
                }

                return X509CertificateReader.ExtractCertificate(responseAsPublicKeyModel.Nokkel);
            }
        }

        public async Task UploadPublicKey(Guid kontoId, string pemString)
        {
            if (string.IsNullOrEmpty(pemString))
            {
                throw new ArgumentException("pemString cannot be null or empty", nameof(pemString));
            }

            var uri = CreatePublicKeyUri(kontoId);

            using (var requestMessage = new HttpRequestMessage(HttpMethod.Put, uri))
            {
                await AddAuthHeaders(requestMessage).ConfigureAwait(false);

                requestMessage.Content = new StringContent(
                    JsonConvert.SerializeObject(new { nokkel = pemString }),
                    Encoding.UTF8,
                    "application/json");

                using (var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        throw new FiksIOSendUnexpectedResponseException(
                            $"Got unexpected HTTP Status code {response.StatusCode} from {uri}. Content: {content}.");
                    }
                }
            }
        }

        private static async Task ThrowIfResponseIsInvalid(HttpResponseMessage response, Uri requestUri)
        {
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new FiksIOSendUnexpectedResponseException(
                    $"Got unexpected HTTP Status code {response.StatusCode} from {requestUri}. Content: {content}.");
            }
        }

        private Uri CreateLookupUri(LookupRequest request)
        {
            var servicePath = $"{_katalogConfiguration.Path}/{LookupEndpoint}";
            var query = $"?{IdentifyerQueryName}={request.Identifikator}&" +
                        $"{MessageProtocolQueryName}={request.Meldingsprotokoll}&" +
                        $"{AccessLevelQueryName}={request.Sikkerhetsniva}";

            return new UriBuilder(
                    _katalogConfiguration.Scheme,
                    _katalogConfiguration.Host,
                    _katalogConfiguration.Port,
                    servicePath,
                    query)
                .Uri;
        }

        private Uri CreateGetKontoUri(Guid kontoId)
        {
            var servicePath = $"{_katalogConfiguration.Path}/{AccountsEndpoint}/{kontoId.ToString()}";
            return new UriBuilder(
                    _katalogConfiguration.Scheme,
                    _katalogConfiguration.Host,
                    _katalogConfiguration.Port,
                    servicePath)
                .Uri;
        }

        private Uri CreateGetKontoStatusUri(Guid kontoId)
        {
            var servicePath = $"{_katalogConfiguration.Path}/{AccountsEndpoint}/{kontoId.ToString()}/{StatusEndpoint}";
            return new UriBuilder(
                    _katalogConfiguration.Scheme,
                    _katalogConfiguration.Host,
                    _katalogConfiguration.Port,
                    servicePath)
                .Uri;
        }

        private Uri CreatePublicKeyUri(Guid receiverAccountId)
        {
            var servicePath =
                $"{_katalogConfiguration.Path}/{AccountsEndpoint}/{receiverAccountId.ToString()}/{PublicKeyEndpoint}";
            return new UriBuilder(
                    _katalogConfiguration.Scheme,
                    _katalogConfiguration.Host,
                    _katalogConfiguration.Port,
                    servicePath)
                .Uri;
        }

        private async Task<T> GetAsModel<T>(Uri requestUri, bool authenticated = true)
        {
            using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri))
            {
                if (authenticated)
                {
                    await AddAuthHeaders(requestMessage).ConfigureAwait(false);
                }

                var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

                await ThrowIfResponseIsInvalid(response, requestUri).ConfigureAwait(false);

                var responseAsJsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<T>(responseAsJsonString);
            }
        }

        private async Task AddAuthHeaders(HttpRequestMessage requestMessage)
        {
            var accessToken = await _maskinportenClient.GetAccessToken(_integrasjonConfiguration.Scope)
                .ConfigureAwait(false);

            requestMessage.Headers.Add("integrasjonId", _integrasjonConfiguration.IntegrasjonId.ToString());
            requestMessage.Headers.Add("integrasjonPassord", _integrasjonConfiguration.IntegrasjonPassord);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Token);
        }
    }
}