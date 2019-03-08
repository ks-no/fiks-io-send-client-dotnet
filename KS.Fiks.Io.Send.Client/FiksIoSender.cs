using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
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

            await ThrowIfUnauthorized(response).ConfigureAwait(false);
            await ThrowIfResponseIsInvalid(response).ConfigureAwait(false);

            return await DeserializeResponse(response).ConfigureAwait(false);
        }

        private async Task SetAuthorizationHeaders()
        {
            foreach (var header in await _authenticationStrategy.GetAuthorizationHeaders().ConfigureAwait(false))
            {
                _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        private async Task<HttpResponseMessage> SendDataWithPost(MessageSpecificationApiModel metaData, Stream data)
        {
            return await _httpClient.PostAsync(
                                        CreateUri(),
                                        CreateRequestContent(metaData, data))
                                    .ConfigureAwait(false);
        }

        private MultipartFormDataContent CreateRequestContent(MessageSpecificationApiModel metaData, Stream data)
        {
            var boundary = Guid.NewGuid().ToString();

            var stringContent = new StringContent(JsonConvert.SerializeObject(metaData), Encoding.UTF8);
            stringContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var dataContent = new StreamContent(data);

            var request = new MultipartFormDataContent(boundary);
            request.Headers.ContentType.Parameters.Clear();
            request.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("boundary", boundary));
            request.Add(new StringContent("Trival test text"));
            //request.Add(stringContent, "\"metadata\"");
            //request.Add(dataContent, "data", Guid.NewGuid().ToString());

            return request;
        }

        private Uri CreateUri()
        {
            var uriBuilder = new UriBuilder(_fiksIoScheme, _fiksIoHost, _fiksIoPort, SendPath);
            return uriBuilder.Uri;
        }

        private async Task ThrowIfUnauthorized(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new FiksIoSendUnauthorizedException(
                    $"Got response Unauthorized (401) from {CreateUri()}. Response: {responseString}.");
            }
        }

        private async Task ThrowIfResponseIsInvalid(HttpResponseMessage response)
        {
            if (response.StatusCode != HttpStatusCode.OK)
            {
                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new FiksIoSendUnexpectedResponseException(
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
                throw new FiksIoSendParseException(
                    $"Unable to parse response from {CreateUri()}. Response: {responseString}.",
                    innerException);
            }
        }
    }
}