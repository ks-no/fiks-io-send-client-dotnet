using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using KS.Fiks.IO.Crypto.Models;
using KS.Fiks.IO.Send.Client.Authentication;
using KS.Fiks.IO.Send.Client.Exceptions;
using KS.Fiks.IO.Send.Client.Models;
using Ks.Fiks.Maskinporten.Client;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.Send.Client.Tests
{
    public class FiksIOSenderTests
    {
        private readonly FiksIOSenderFixture _fixture;

        public FiksIOSenderTests()
        {
            _fixture = new FiksIOSenderFixture();
        }

        [Fact]
        public async Task ReturnsSentMessageApiModel()
        {
            var sut = _fixture.CreateSut();
            var result = await sut.Send(_fixture.DefaultMessage, new MemoryStream()).ConfigureAwait(false);

            result.ShouldBeOfType<SendtMeldingApiModel>();
        }

        [Fact]
        public async Task ReturnsSentMessageApiModelIfNoData()
        {
            var sut = _fixture.CreateSut();
            var result = await sut.Send(_fixture.DefaultMessage).ConfigureAwait(false);

            result.ShouldBeOfType<SendtMeldingApiModel>();
        }

        [Fact]
        public async Task SendsAPostRequestWithExpectedHost()
        {
            var host = "test.host.com";

            var sut = _fixture.WithHost(host).CreateSut();

            var result = await sut.Send(_fixture.DefaultMessage, new MemoryStream()).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.Host == host),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendsAPostRequestWithExpectedPort()
        {
            var port = 8081;

            var sut = _fixture.WithPort(port).CreateSut();

            var result = await sut.Send(_fixture.DefaultMessage, new MemoryStream()).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.Port == port),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendsAPostRequestWithExpectedScheme()
        {
            var scheme = "https";

            var sut = _fixture.WithScheme(scheme).CreateSut();

            var result = await sut.Send(_fixture.DefaultMessage, new MemoryStream()).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.Scheme == scheme),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendAPostRequestToExpectedPath()
        {
            var expectedRequestPath = "/fiksio/api/v1/send";

            var sut = _fixture.WithPath(expectedRequestPath).CreateSut();

            var result = await sut.Send(_fixture.DefaultMessage, new MemoryStream()).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.PathAndQuery == expectedRequestPath),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendsExpectedMetadata()
        {
            var sut = _fixture.CreateSut();

            var model = new MeldingSpesifikasjonApiModel(
                avsenderKontoId: Guid.NewGuid(),
                mottakerKontoId: Guid.NewGuid(),
                meldingType: "messageType",
                ttl: 100,
                headere: new Dictionary<string, string>(),
                svarPaMelding: Guid.NewGuid());
            var serializedModel = JsonConvert.SerializeObject(model);
            var result = await sut.Send(model, new MemoryStream()).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    TestHelper.GetPartContent(req, "metadata").Result == serializedModel),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendsExpectedStringAsFile()
        {
            var sut = _fixture.CreateSut();

            var text = "Test text";

            using (var memoryStream = new MemoryStream(Encoding.ASCII.GetBytes(text)))
            {
                var result = await sut.Send(_fixture.DefaultMessage, memoryStream).ConfigureAwait(false);

                _fixture.HttpMessageHandleMock.Protected().Verify(
                    "SendAsync",
                    Times.Exactly(1),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        TestHelper.GetPartContent(req, "data").Result == text),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [Fact]
        public async Task SendsExpectedFile()
        {
            var sut = _fixture.CreateSut();

            var fileText = File.ReadAllText("./testfile.txt");

            using (var memoryStream = new FileStream("./testfile.txt", FileMode.Open))
            {

                var result = await sut.Send(_fixture.DefaultMessage, memoryStream).ConfigureAwait(false);

                _fixture.HttpMessageHandleMock.Protected().Verify(
                    "SendAsync",
                    Times.Exactly(1),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        TestHelper.GetPartContent(req, "data").Result == fileText),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [Fact]
        public async Task SendsFileWithUuidFilename()
        {
            var sut = _fixture.CreateSut();

            using (var memoryStream = new FileStream("./testfile.txt", FileMode.Open))
            {

                var result = await sut.Send(_fixture.DefaultMessage, memoryStream).ConfigureAwait(false);

                Guid tmp;

                _fixture.HttpMessageHandleMock.Protected().Verify(
                    "SendAsync",
                    Times.Exactly(1),
                    ItExpr.Is<HttpRequestMessage>(req =>
                        Guid.TryParse(TestHelper.GetFilename(req, "data"), out tmp)),
                    ItExpr.IsAny<CancellationToken>());
            }
        }

        [Fact]
        public async Task ReturnsExpectedSentMessageApiModel()
        {
            var expectedResult = new SendtMeldingApiModel
            {
                MeldingId = Guid.NewGuid(),
                MeldingType = "type",
                AvsenderKontoId = Guid.NewGuid(),
                MottakerKontoId = Guid.NewGuid(),
                Ttl = 10,
                DokumentlagerId = Guid.NewGuid(),
                SvarPaMelding = Guid.NewGuid()
            };

            var sut = _fixture.WithReturnValue(expectedResult).CreateSut();
            var result = await sut.Send(_fixture.DefaultMessage, new MemoryStream()).ConfigureAwait(false);
            result.MeldingId.ShouldBe(expectedResult.MeldingId);
            result.MeldingType.ShouldBe(expectedResult.MeldingType);
            result.AvsenderKontoId.ShouldBe(expectedResult.AvsenderKontoId);
            result.MottakerKontoId.ShouldBe(expectedResult.MottakerKontoId);
            result.Ttl.ShouldBe(expectedResult.Ttl);
            result.DokumentlagerId.ShouldBe(expectedResult.DokumentlagerId);
            result.SvarPaMelding.ShouldBe(expectedResult.SvarPaMelding);
        }

        [Fact]
        public async Task ReturnsExpectedSendMessageApiModelWhenDokumentlagerIdIsNotSet()
        {
            var expectedResultAsJson = "{\"meldingId\":\"49d4f267-4d2d-46c4-9c0d-55b37ddff50d\"," +
                                       "\"meldingType\":\"fiks-io-send-test\"," +
                                       "\"avsenderKontoId\":\"a6ac54b1-6ab5-413d-8ba5-aac64bbeff08\"," +
                                       "\"mottakerKontoId\":\"a6ac54b1-6ab5-413d-8ba5-aac64bbeff08\"," +
                                       "\"ttl\":3434,\"dokumentlagerId\":null," +
                                       "\"headere\":{\"MyHeader\":\"MyValue\"}," +
                                       "\"svarPaMelding\":\"38bc7d50-08e1-4cb6-b2b1-7b9805ca8def\"}";

            var sut = _fixture.WithReturnValueAsJson(expectedResultAsJson).CreateSut();
            var result = await sut.Send(_fixture.DefaultMessage, new MemoryStream()).ConfigureAwait(false);
        }

        [Fact]
        public async Task SetsAuthenticationHeaders()
        {
            var authorizationHeaders = new Dictionary<string, string>
            {
                {"AUTHORIZATION", "BEARER alkdjfhsdgkjsdhfkjsdhg"},
                {"integrationId", "myId"}
            };

            var sut = _fixture.WithAuthorizationHeaders(authorizationHeaders).CreateSut();

            var result = await sut.Send(_fixture.DefaultMessage, new MemoryStream()).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Headers.GetValues("AUTHORIZATION").FirstOrDefault() ==
                    authorizationHeaders["AUTHORIZATION"] &&
                    req.Headers.GetValues("integrationId").FirstOrDefault() ==
                    authorizationHeaders["integrationId"]),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task ThrowsExceptionIfResponseIsNotParsable()
        {
            var sut = _fixture.WithInvalidReturnValue().CreateSut();
            await Assert.ThrowsAsync<FiksIOSendParseException>(
                            async () => await sut.Send(_fixture.DefaultMessage, new MemoryStream())
                                                 .ConfigureAwait(false))
                        .ConfigureAwait(false);
        }

        [Theory]
        [InlineData(HttpStatusCode.NotFound)]
        [InlineData(HttpStatusCode.InternalServerError)]
        [InlineData(HttpStatusCode.NoContent)]
        [InlineData(HttpStatusCode.Redirect)]
        [InlineData(HttpStatusCode.Forbidden)]
        [InlineData(HttpStatusCode.UnavailableForLegalReasons)]
        [InlineData(HttpStatusCode.OK)]
        public async Task ThrowsExceptionIfStatusCodeIsNot202(HttpStatusCode statusCode)
        {
            var sut = _fixture.WithStatusCode(statusCode).CreateSut();

            await Assert.ThrowsAsync<FiksIOSendUnexpectedResponseException>(
                            async () => await sut.Send(_fixture.DefaultMessage, new MemoryStream())
                                                 .ConfigureAwait(false))
                        .ConfigureAwait(false);
        }

        [Fact]
        public async Task ThrowsUnauthorizedExceptionIfStatusCodeIs401()
        {
            var sut = _fixture.WithStatusCode(HttpStatusCode.Unauthorized).CreateSut();

            await Assert.ThrowsAsync<FiksIOSendUnauthorizedException>(
                            async () => await sut.Send(_fixture.DefaultMessage, new MemoryStream())
                                                 .ConfigureAwait(false))
                        .ConfigureAwait(false);
        }

        [Theory]
        [InlineData("send1")]
        [InlineData("send2")]
        [InlineData("sendEncrypted1")]
        [InlineData("sendEncrypted2")]
        public async Task AllEndpointsAllowCancellation(string methodId)
        {
            // Arrange
            var maskinportenMock = new Mock<IMaskinportenClient>();
            maskinportenMock.Setup(x => x.GetAccessToken(It.IsAny<string>())).ReturnsAsync(new MaskinportenToken(string.Empty, 300));
            var authStrategy = new IntegrasjonAuthenticationStrategy(maskinportenMock.Object, Guid.Empty, string.Empty);
            var sut = _fixture.CreateSut(authStrategy);
            var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            Func<Task<SendtMeldingApiModel>> method = methodId switch
            {
                "send1" => () => sut.Send(_fixture.DefaultMessage, cts.Token),
                "send2" => () => sut.Send(_fixture.DefaultMessage, PayloadWrapper.CreateDummy().Payload, cts.Token),
                "sendEncrypted1" => () => sut.SendWithEncryptedData(_fixture.DefaultMessage, PayloadWrapper.CreateDummy(), cts.Token),
                "sendEncrypted2" => () => sut.SendWithEncryptedData(_fixture.DefaultMessage, [PayloadWrapper.CreateDummy()], cts.Token),
                _ => throw new ArgumentOutOfRangeException(nameof(methodId))
            };

            // Act
            var ex = await Record.ExceptionAsync(method);

            // Assert
            Assert.IsType<TaskCanceledException>(ex);
            Assert.Contains("canceled", ex.Message, StringComparison.OrdinalIgnoreCase);
        }
    }

    internal sealed record PayloadWrapper(string Filename, Stream Payload) : IPayload
    {
        public static PayloadWrapper CreateDummy()
        {
            return new PayloadWrapper("dummy.txt", new MemoryStream("The content"u8.ToArray()));
        }
    }
}