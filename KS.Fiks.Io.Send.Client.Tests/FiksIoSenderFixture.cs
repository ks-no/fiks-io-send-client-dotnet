using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace KS.Fiks.Io.Send.Client.Tests
{
    public class FiksIoSenderFixture
    {
        
        private string _fiksIoScheme;
        private string _fiksIoHost;
        private int _fiksIoPort;
        private HttpStatusCode _statusCode;
        private SentMessageApiModel _returnValue;
        public FiksIoSenderFixture()
        {
            SetDefaultValues();
            AuthenticationStrategyMock = new Mock<IAuthenticationStrategy>();
            HttpMessageHandleMock = new Mock<HttpMessageHandler>();
        }
        
        public Mock<IAuthenticationStrategy> AuthenticationStrategyMock { get; }
        public Mock<HttpMessageHandler> HttpMessageHandleMock { get; }
        
        public FiksIoSender CreateSut()
        {
            SetupMocks();
            return new FiksIoSender(_fiksIoScheme, _fiksIoHost,_fiksIoPort, AuthenticationStrategyMock.Object, new HttpClient(HttpMessageHandleMock.Object));
        }

        public FiksIoSenderFixture WithScheme(string scheme)
        {
            _fiksIoScheme = scheme;
            return this;
        }

        public FiksIoSenderFixture WithHost(string host)
        {
            _fiksIoHost = host;
            return this;
        }

        public FiksIoSenderFixture WithPort(int port)
        {
            _fiksIoPort = port;
            return this;
        }
        
        public FiksIoSenderFixture WithStatusCode(HttpStatusCode code)
        {
            _statusCode = code;
            return this;
        }

        public FiksIoSenderFixture WithReturnValue(SentMessageApiModel value)
        {
            _returnValue = value;
            return this;
        }

        private void SetDefaultValues()
        {
            _fiksIoHost = "test.no";
            _fiksIoScheme = "http";
            _fiksIoPort = 8084;
            _statusCode = HttpStatusCode.OK;
            _returnValue = new SentMessageApiModel();
        }

        private void SetupMocks()
        {
            SetHttpResponse();
        }
        
        private void SetHttpResponse()
        {
            var responseMessage = new HttpResponseMessage()
            {
                StatusCode = _statusCode,
                Content = new StringContent(GenerateJsonResponse()),
            };

            HttpMessageHandleMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(responseMessage)
                .Verifiable();
        }

        private string GenerateJsonResponse()
        {
            return JsonConvert.SerializeObject(_returnValue);
        }
    }
}