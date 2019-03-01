using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
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

            var response = await _httpClient.PostAsync(
                                                CreateUri(),
                                                CreateRequestContent(metaData, data))
                                            .ConfigureAwait(false);

            return await DeserializeResponse(response).ConfigureAwait(false);
        }

        private static async Task<SentMessageApiModel> DeserializeResponse(HttpResponseMessage response)
        {
            return JsonConvert.DeserializeObject<SentMessageApiModel>(
                await response.Content.ReadAsStringAsync().ConfigureAwait(false));
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