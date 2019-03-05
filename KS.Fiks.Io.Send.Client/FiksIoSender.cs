using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using KS.Fiks.Io.Send.Client.Exceptions;
using Newtonsoft.Json;

namespace KS.Fiks.Io.Send.Client
{
    public class FiksIoSender : IFiksIOSender
    {
        private const string BasePath = "/svarinn2/api/v1";
        private const string SendPath = BasePath + "/send";

        private readonly string _fiksIoScheme;
        private readonly string _fiksIoHost;
        private readonly int _fiksIoPort;
        private readonly HttpClient _httpClient;

        private IAuthenticationStrategy _authenticationStrategy;

        public FiksIoSender(
            string fiksIoScheme,
            string fiksIoHost,
            int fiksIoPort,
            IAuthenticationStrategy authenticationStrategy,
            HttpClient httpClient = null)
        {
            _fiksIoScheme = fiksIoScheme;
            _fiksIoHost = fiksIoHost;
            _fiksIoPort = fiksIoPort;
            _authenticationStrategy = authenticationStrategy;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<SentMessageApiModel> Send(MessageSpecificationApiModel metaData, Stream data)
        {
            await SetAuthorizationHeaders().ConfigureAwait(false);

            var response = await SendDataWithPost(metaData, data).ConfigureAwait(false);

            return await DeserializeResponse(response).ConfigureAwait(false);
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
                throw new FiksIoParseException(
                    $"Unable to parse response from {_fiksIoScheme}/{_fiksIoHost}:{_fiksIoPort}/{SendPath}. Response: {responseString}.", innerException);
            }
        }

        private async Task<HttpResponseMessage> SendDataWithPost(MessageSpecificationApiModel metaData, Stream data)
        {
            return await _httpClient.PostAsync(
                                        CreateUri(),
                                        CreateRequestContent(metaData, data))
                                    .ConfigureAwait(false);
        }

        private async Task SetAuthorizationHeaders()
        {
            foreach (var header in await _authenticationStrategy.GetAuthorizationHeaders().ConfigureAwait(false))
            {
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        private MultipartContent CreateRequestContent(MessageSpecificationApiModel metaData, Stream data)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(metaData));
            stringContent.Headers.Add("name", "metadata");
            var dataContent = new StreamContent(data);
            dataContent.Headers.Add("name", "data");
            dataContent.Headers.Add("filename", Guid.NewGuid().ToString());

            var request = new MultipartContent { stringContent, dataContent };

            return request;
        }

        private Uri CreateUri()
        {
            var uriBuilder = new UriBuilder(_fiksIoScheme, _fiksIoHost, _fiksIoPort, SendPath);
            return uriBuilder.Uri;
        }
    }
}