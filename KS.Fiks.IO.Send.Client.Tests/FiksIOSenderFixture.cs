using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        private SentMessageApiModel _returnValue;
        private string _returnValueAsJson;
        private Dictionary<string, string> _authorizationHeaders;
        private bool _useInvalidReturnValue = false;

        public FiksIOSenderFixture()
        {
            SetDefaultValues();
            AuthenticationStrategyMock = new Mock<IAuthenticationStrategy>();
            HttpMessageHandleMock = new Mock<HttpMessageHandler>();
        }

        public Mock<IAuthenticationStrategy> AuthenticationStrategyMock { get; }

        public Mock<HttpMessageHandler> HttpMessageHandleMock { get; }

        public MessageSpecificationApiModel DefaultMessage =>
            new MessageSpecificationApiModel(Guid.NewGuid(), Guid.NewGuid(), "defaultType", 100, null);

        public FiksIOSender CreateSut()
        {
            SetupMocks();
            var configuration = new FiksIOSenderConfiguration(_path, _scheme, _host, _port);
            return new FiksIOSender(
                configuration,
                AuthenticationStrategyMock.Object,
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

        public FiksIOSenderFixture WithReturnValue(SentMessageApiModel value)
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
            _returnValue = new SentMessageApiModel();
            _authorizationHeaders = new Dictionary<string, string>();
        }

        private void SetupMocks()
        {
            SetHttpResponse();
            SetupAuthenticationStrategyMock();
        }

        private void SetHttpResponse()
        {
            var responseMessage = new HttpResponseMessage()
            {
                StatusCode = _statusCode,
                Content = _useInvalidReturnValue ? GenerateInvalidResponse() : GenerateJsonResponse()
            };

            HttpMessageHandleMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage)
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