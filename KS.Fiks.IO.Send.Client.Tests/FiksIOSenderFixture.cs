using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Crypto.Configuration;
using KS.Fiks.IO.Send.Client.Authentication;
using KS.Fiks.IO.Send.Client.Configuration;
using KS.Fiks.IO.Send.Client.Models;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Send.Client.Tests
{
    public class FiksIOSenderFixture
    {
        private string _path;
        private string _scheme;
        private string _host;
        private int _port;
        private HttpStatusCode _statusCode;
        private SendtMeldingApiModel _returnValue;
        private string _returnValueAsJson;
        private Dictionary<string, string> _authorizationHeaders;
        private bool _useInvalidReturnValue = false;
        private IntegrasjonConfiguration _integrasjonConfiguration;
        private AsiceSigningConfiguration _asiceSigningConfiguration;

        public FiksIOSenderFixture()
        {
            SetDefaultValues();
            HttpMessageHandleMock = new Mock<HttpMessageHandler>();
            AuthenticationStrategyMock = new Mock<IAuthenticationStrategy>();
        }

        public Mock<IAuthenticationStrategy> AuthenticationStrategyMock { get; }

        public Mock<HttpMessageHandler> HttpMessageHandleMock { get; }

        public Guid SenderAccountId { get; } = Guid.NewGuid();

        public Guid ReceiverAccountId { get; } = Guid.NewGuid();

        public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

        public MeldingSpesifikasjonApiModel DefaultMessage =>
            new(
                avsenderKontoId: SenderAccountId,
                mottakerKontoId: ReceiverAccountId,
                meldingType: "defaultType",
                ttl: 100,
                headere: new Dictionary<string, string> {{ "My_Header", "My_Value" }});

        public FiksIOSender CreateSut(IAuthenticationStrategy authenticationStrategy = null)
        {
            SetupMocks();
            var configuration = new FiksIOSenderConfiguration(
                _path,
                _scheme,
                _host,
                _port,
                _asiceSigningConfiguration,
                _integrasjonConfiguration);
            return new FiksIOSender(
                configuration,
                authenticationStrategy ?? AuthenticationStrategyMock.Object,
                new HttpClient(HttpMessageHandleMock.Object));
        }

        public FiksIOSenderFixture WithPath(string path)
        {
            _path = path;
            return this;
        }

        public FiksIOSenderFixture WithScheme(string scheme)
        {
            _scheme = scheme;
            return this;
        }

        public FiksIOSenderFixture WithHost(string host)
        {
            _host = host;
            return this;
        }

        public FiksIOSenderFixture WithPort(int port)
        {
            _port = port;
            return this;
        }

        public FiksIOSenderFixture WithStatusCode(HttpStatusCode code)
        {
            _statusCode = code;
            return this;
        }

        public FiksIOSenderFixture WithReturnValue(SendtMeldingApiModel value)
        {
            _returnValue = value;
            return this;
        }

        public FiksIOSenderFixture WithReturnValueAsJson(string value)
        {
            _returnValueAsJson = value;
            return this;
        }

        public FiksIOSenderFixture WithAuthorizationHeaders(Dictionary<string, string> value)
        {
            _authorizationHeaders = value;
            return this;
        }

        public FiksIOSenderFixture WithInvalidReturnValue()
        {
            _useInvalidReturnValue = true;
            return this;
        }

        public FiksIOSenderFixture WithAsiceSigningConfiguration(AsiceSigningConfiguration value)
        {
            _asiceSigningConfiguration = value;
            return this;
        }

        public FiksIOSenderFixture WithIntegrasjonConfiguration(IntegrasjonConfiguration value)
        {
            _integrasjonConfiguration = value;
            return this;
        }

        private static StringContent GenerateInvalidResponse()
        {
            return new StringContent(">DSFSV#%¤DFGHV___XCXV132<>");
        }

        private void SetDefaultValues()
        {
            _returnValueAsJson = string.Empty;
            _host = "test.no";
            _scheme = "http";
            _port = 8084;
            _statusCode = HttpStatusCode.Accepted;
            _returnValue = new SendtMeldingApiModel();
            _authorizationHeaders = new Dictionary<string, string>();
            _asiceSigningConfiguration = new AsiceSigningConfiguration(TestHelper.GetDummyCert(TimeProvider));
            _integrasjonConfiguration = new IntegrasjonConfiguration(Guid.Empty, string.Empty);
        }

        private void SetupMocks()
        {
            SetHttpResponse();
            SetupAuthenticationStrategyMock();
        }

        private void SetHttpResponse()
        {
            var isPubKeyRequest = new Func<HttpRequestMessage, bool>(req =>
                req.RequestUri!.AbsoluteUri.Contains("katalog/api/v1", StringComparison.OrdinalIgnoreCase));

            HttpMessageHandleMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => isPubKeyRequest(req)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = JsonContent.Create(TestHelper.GetDummyPublicKey(TimeProvider))
                });

            HttpMessageHandleMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => !isPubKeyRequest(req)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage()
                {
                    StatusCode = _statusCode,
                    Content = _useInvalidReturnValue ? GenerateInvalidResponse() : GenerateJsonResponse()
                })
                .Verifiable();
        }

        private StringContent GenerateJsonResponse()
        {
            if (_returnValueAsJson.Length > 0)
            {
                return new StringContent(_returnValueAsJson);
            }

            return new StringContent(JsonConvert.SerializeObject(_returnValue));
        }

        private void SetupAuthenticationStrategyMock()
        {
            AuthenticationStrategyMock.Setup(x => x.GetAuthorizationHeaders()).ReturnsAsync(_authorizationHeaders);
        }
    }
}