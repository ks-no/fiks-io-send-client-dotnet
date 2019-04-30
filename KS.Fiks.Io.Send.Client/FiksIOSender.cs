using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using KS.Fiks.IO.Send.Client.Configuration;
using KS.Fiks.IO.Send.Client.Exceptions;
using Ks.Fiks.Maskinporten.Client;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Send.Client
{
    public class FiksIOSender : IFiksIOSender
    {
        private readonly FiksIOSenderConfiguration _configuration;
        private readonly HttpClient _httpClient;

        private readonly IAuthenticationStrategy _authenticationStrategy;

        public FiksIOSender(
            FiksIOSenderConfiguration configuration,
            IAuthenticationStrategy authenticationStrategy,
            HttpClient httpClient = null)
        {
            _configuration = configuration;
            _authenticationStrategy = authenticationStrategy;
            _httpClient = httpClient ?? new HttpClient();
        }

        public FiksIOSender(
            FiksIOSenderConfiguration configuration,
            IMaskinportenClient maskinportenClient,
            Guid integrastionId,
            string integrationPassword,
            HttpClient httpClient = null)
            : this(
                configuration,
                new IntegrasjonAuthenticationStrategy(maskinportenClient, integrastionId, integrationPassword),
                httpClient)
        {
        }

        public async Task<SentMessageApiModel> Send(MessageSpecificationApiModel metaData, Stream data)
        {
            var response = await SendDataWithPost(metaData, data).ConfigureAwait(false);

            await ThrowIfUnauthorized(response).ConfigureAwait(false);
            await ThrowIfResponseIsInvalid(response).ConfigureAwait(false);

            return await DeserializeResponse(response).ConfigureAwait(false);
        }

        private async Task<HttpResponseMessage> SendDataWithPost(MessageSpecificationApiModel metaData, Stream data)
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

        private MultipartFormDataContent CreateRequestContent(MessageSpecificationApiModel metaData, Stream data)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(metaData), Encoding.UTF8);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var dataContent = new StreamContent(data);

            var request = new MultipartFormDataContent
            {
                {stringContent, "metadata"},
                {dataContent, "data", Guid.NewGuid().ToString()}
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

        private async Task<SentMessageApiModel> DeserializeResponse(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            try
            {
                return JsonConvert.DeserializeObject<SentMessageApiModel>(responseString);
            }
            catch (Exception innerException)
            {
                throw new FiksIOSendParseException(
                    $"Unable to parse response from {CreateUri()}. Response: {responseString}.",
                    innerException);
            }
        }
    }
}