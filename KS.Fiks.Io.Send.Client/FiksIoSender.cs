using System;
using System.IO;
using System.Net.Http;
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
        private IAuthenticationStrategy _authenticationStrategy;
        private readonly HttpClient _httpClient;

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
            var uriBuilder = new UriBuilder(_fiksIoScheme, _fiksIoHost, _fiksIoPort, SendPath);
            var stringContent = new StringContent(JsonConvert.SerializeObject(metaData));
            stringContent.Headers.Add("name", "metadata");
            var dataContent = new StreamContent(data);
            dataContent.Headers.Add("name", "data");
            dataContent.Headers.Add("filename", Guid.NewGuid().ToString());
            var multiPart = new MultipartContent {stringContent, dataContent};
            var result = await _httpClient.PostAsync(uriBuilder.Uri, multiPart);
            return JsonConvert.DeserializeObject<SentMessageApiModel>(await result.Content.ReadAsStringAsync());
        }
    }
}