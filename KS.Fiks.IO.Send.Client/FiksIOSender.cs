using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using KS.Fiks.IO.Crypto.Asic;
using KS.Fiks.IO.Crypto.Models;
using KS.Fiks.IO.Send.Client.Authentication;
using KS.Fiks.IO.Send.Client.Catalog;
using KS.Fiks.IO.Send.Client.Configuration;
using KS.Fiks.IO.Send.Client.Exceptions;
using KS.Fiks.IO.Send.Client.Models;
using Ks.Fiks.Maskinporten.Client;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Send.Client
{
    public class FiksIOSender : IFiksIOSender
    {
        private readonly FiksIOSenderConfiguration _configuration;
        private readonly HttpClient _httpClient;

        private readonly IAuthenticationStrategy _authenticationStrategy;
        private readonly IAsicEncrypter _asicEncrypter;
        private readonly IPublicKeyProvider _publicKeyProvider;

        public FiksIOSender(
            FiksIOSenderConfiguration configuration,
            IAuthenticationStrategy authenticationStrategy,
            HttpClient httpClient = null)
        {
            _configuration = configuration;
            _authenticationStrategy = authenticationStrategy;
            _httpClient = httpClient ?? new HttpClient();

            if (_configuration.IntegrasjonConfiguration != null &&
                _authenticationStrategy is IntegrasjonAuthenticationStrategy integrasjonAuthenticationStrategy)
            {
                _publicKeyProvider = new CatalogPublicKeyProvider(
                    new CatalogHandler(
                        new KatalogConfiguration(host: configuration?.Host),
                        new IntegrasjonConfiguration(
                            _configuration.IntegrasjonConfiguration.IntegrasjonId, 
                            _configuration.IntegrasjonConfiguration.IntegrasjonPassord),
                        integrasjonAuthenticationStrategy.MaskinportenClient,
                        _httpClient));
            }

            if (_configuration.AsiceSigningConfiguration != null)
            {
                _asicEncrypter = new AsicEncrypter(
                    new AsiceBuilderFactory(),
                    new EncryptionServiceFactory(),
                    AsicSigningCertificateHolderFactory.Create(_configuration.AsiceSigningConfiguration));
            }
        }

        public FiksIOSender(
            FiksIOSenderConfiguration configuration,
            IMaskinportenClient maskinportenClient,
            HttpClient httpClient = null)
            : this(
                configuration,
                new IntegrasjonAuthenticationStrategy(maskinportenClient, configuration),
                httpClient)
        {
        }

        [Obsolete("Use configuration object instead of integrasjonId and integrasjonPassord parameters")]
        public FiksIOSender(
            FiksIOSenderConfiguration configuration,
            IMaskinportenClient maskinportenClient,
            Guid integrasjonId,
            string integrasjonPassord,
            HttpClient httpClient = null)
            : this(
                configuration,
                new IntegrasjonAuthenticationStrategy(maskinportenClient, integrasjonId, integrasjonPassord),
                httpClient)
        {
            _publicKeyProvider = new CatalogPublicKeyProvider(
                new CatalogHandler(
                    new KatalogConfiguration(host: configuration?.Host),
                    new IntegrasjonConfiguration(integrasjonId, integrasjonPassord),
                    maskinportenClient,
                    _httpClient));

            if (configuration.AsiceSigningConfiguration != null)
            {
                _asicEncrypter = new AsicEncrypter(
                    new AsiceBuilderFactory(),
                    new EncryptionServiceFactory(),
                    AsicSigningCertificateHolderFactory.Create(configuration.AsiceSigningConfiguration));
            }
        }

        public async Task<SendtMeldingApiModel> SendWithEncryptedData(
            MeldingSpesifikasjonApiModel metaData,
            IPayload payload)
        {
            return await SendEncryptedData(metaData, new List<IPayload> { payload }).ConfigureAwait(false);
        }

        public async Task<SendtMeldingApiModel> SendWithEncryptedData(
            MeldingSpesifikasjonApiModel metaData,
            IList<IPayload> payload)
        {
            return await SendEncryptedData(metaData, payload).ConfigureAwait(false);
        }

        private async Task<SendtMeldingApiModel> SendEncryptedData(
            MeldingSpesifikasjonApiModel metaData,
            IList<IPayload> payload)
        {
            if (_publicKeyProvider == null || _asicEncrypter == null)
            {
                throw new FiksIOSendEncryptionException("Cannot send encrypted data. Encryption is not configured.");
            }

            var encryptedPayload = await GetEncryptedPayload(metaData.MottakerKontoId, payload).ConfigureAwait(false);
            return await Send(metaData, encryptedPayload).ConfigureAwait(false);
        }

        public async Task<SendtMeldingApiModel> Send(MeldingSpesifikasjonApiModel metaData, Stream data)
        {
            var response = await SendDataWithPost(metaData, data).ConfigureAwait(false);

            await ThrowIfUnauthorized(response).ConfigureAwait(false);
            await ThrowIfResponseIsInvalid(response).ConfigureAwait(false);

            return await DeserializeResponse(response).ConfigureAwait(false);
        }

        public async Task<SendtMeldingApiModel> Send(MeldingSpesifikasjonApiModel metaData)
        {
            return await Send(metaData, null).ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> SendDataWithPost(MeldingSpesifikasjonApiModel metaData, Stream data)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, CreateUri());
            foreach (var keyValuePair in await _authenticationStrategy
                         .GetAuthorizationHeaders().ConfigureAwait(false))
            {
                requestMessage.Headers.Add(keyValuePair.Key, keyValuePair.Value);
            }

            requestMessage.Content = CreateRequestContent(metaData, data);

            return await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
        }

        private MultipartFormDataContent CreateRequestContent(MeldingSpesifikasjonApiModel metaData, Stream data)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(metaData), Encoding.UTF8);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            if (data == null)
            {
                return new MultipartFormDataContent
                {
                    { stringContent, "metadata" }
                };
            }

            var dataContent = new StreamContent(data);

            var request = new MultipartFormDataContent
            {
                { stringContent, "metadata" },
                { dataContent, "data", Guid.NewGuid().ToString() }
            };

            return request;
        }

        private Uri CreateUri()
        {
            var uriBuilder = new UriBuilder(
                _configuration.Scheme,
                _configuration.Host,
                _configuration.Port,
                _configuration.Path);
            return uriBuilder.Uri;
        }

        private async Task ThrowIfUnauthorized(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new FiksIOSendUnauthorizedException(
                    $"Got response Unauthorized (401) from {CreateUri()}. Response: {responseString}.");
            }
        }

        private async Task ThrowIfResponseIsInvalid(HttpResponseMessage response)
        {
            if (response.StatusCode != HttpStatusCode.Accepted)
            {
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new FiksIOSendUnexpectedResponseException(
                    $"Got unexpected HTTP Status code {response.StatusCode} from {CreateUri()}. Response: {responseString}.");
            }
        }

        private async Task<SendtMeldingApiModel> DeserializeResponse(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            try
            {
                return JsonConvert.DeserializeObject<SendtMeldingApiModel>(responseString);
            }
            catch (Exception innerException)
            {
                throw new FiksIOSendParseException(
                    $"Unable to parse response from {CreateUri()}. Response: {responseString}.",
                    innerException);
            }
        }

        private async Task<Stream> GetEncryptedPayload(Guid mottakerKontoId, IPayload payload)
        {
            if (payload is null)
            {
                return null;
            }

            return await EncryptPayload(mottakerKontoId, new List<IPayload> { payload }).ConfigureAwait(false);
        }

        private async Task<Stream> GetEncryptedPayload(Guid mottakerKontoId, IList<IPayload> payload)
        {
            if (payload.Count == 0)
            {
                return null;
            }

            return await EncryptPayload(mottakerKontoId, payload).ConfigureAwait(false);
        }

        private async Task<Stream> EncryptPayload(Guid mottakerKontoId, IList<IPayload> payload)
        {
            var receiverPublicKey = await _publicKeyProvider.GetPublicKey(mottakerKontoId).ConfigureAwait(false);
            var encryptedPayload = _asicEncrypter.Encrypt(receiverPublicKey, payload);
            encryptedPayload.Seek(0, SeekOrigin.Begin);
            return encryptedPayload;
        }
    }
}