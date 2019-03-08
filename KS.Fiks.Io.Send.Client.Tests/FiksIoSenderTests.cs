using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using KS.Fiks.Io.Send.Client.Exceptions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace KS.Fiks.Io.Send.Client.Tests
{
    public class FiksIoSenderTests
    {
        private readonly FiksIoSenderFixture _fixture;

        public FiksIoSenderTests()
        {
            _fixture = new FiksIoSenderFixture();
        }

        /*
        [Fact]
        public async Task ReturnsSentMessageApiModel()
        {
            var sut = _fixture.CreateSut();
            var result = await sut.Send(new MessageSpecificationApiModel(), new MemoryStream()).ConfigureAwait(false);

            result.Should().BeOfType<SentMessageApiModel>();
        }

        [Fact]
        public async Task SendsAPostRequestWithExpectedHost()
        {
            var host = "test.host.com";

            var sut = _fixture.WithHost(host).CreateSut();

            var result = await sut.Send(new MessageSpecificationApiModel(), new MemoryStream()).ConfigureAwait(false);

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

            var result = await sut.Send(new MessageSpecificationApiModel(), new MemoryStream()).ConfigureAwait(false);

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

            var result = await sut.Send(new MessageSpecificationApiModel(), new MemoryStream()).ConfigureAwait(false);

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
            var expectedRequestPath = "/svarinn2/api/v1/send";

            var sut = _fixture.CreateSut();

            var result = await sut.Send(new MessageSpecificationApiModel(), new MemoryStream()).ConfigureAwait(false);

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

            var model = new MessageSpecificationApiModel
            {
                AvsenderKontoId = Guid.NewGuid(),
                MottakerKontoId = Guid.NewGuid(),
                MeldingType = "A type",
                SvarPaMelding = Guid.NewGuid(),
                Ttl = 100
            };
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
        public async Task SendsExpectedFile()
        {
            var sut = _fixture.CreateSut();

            var memoryStream = new FileStream("./testfile.txt", FileMode.Open);

            var fileText = File.ReadAllText("./testfile.txt");

            var result = await sut.Send(new MessageSpecificationApiModel(), memoryStream).ConfigureAwait(false);

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    TestHelper.GetPartContent(req, "data").Result == fileText),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendsFileWithUuidFilename()
        {
            var sut = _fixture.CreateSut();

            var memoryStream = new FileStream("./testfile.txt", FileMode.Open);

            var result = await sut.Send(new MessageSpecificationApiModel(), memoryStream).ConfigureAwait(false);

            Guid tmp;

            _fixture.HttpMessageHandleMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.Is<HttpRequestMessage>(req =>
                    Guid.TryParse(TestHelper.GetFilename(req, "data"), out tmp)),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task ReturnsExpectedSentMessageApiModel()
        {
            var expectedResult = new SentMessageApiModel
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
            var result = await sut.Send(new MessageSpecificationApiModel(), new MemoryStream()).ConfigureAwait(false);
            result.MeldingId.Should().Be(expectedResult.MeldingId);
            result.MeldingType.Should().Be(expectedResult.MeldingType);
            result.AvsenderKontoId.Should().Be(expectedResult.AvsenderKontoId);
            result.MottakerKontoId.Should().Be(expectedResult.MottakerKontoId);
            result.Ttl.Should().Be(expectedResult.Ttl);
            result.DokumentlagerId.Should().Be(expectedResult.DokumentlagerId);
            result.SvarPaMelding.Should().Be(expectedResult.SvarPaMelding);
        }

        [Fact]
        public async Task SetsAuthenticationHeaders()
        {
            var authorizationHeaders = new Dictionary<string, string>
            {
                { "AUTHORIZATION", "BEARER alkdjfhsdgkjsdhfkjsdhg" },
                { "integrationId", "myId" }
            };

            var sut = _fixture.WithAuthorizationHeaders(authorizationHeaders).CreateSut();

            var result = await sut.Send(new MessageSpecificationApiModel(), new MemoryStream()).ConfigureAwait(false);

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
            await Assert.ThrowsAsync<FiksIoSendParseException>(
                            async () => await sut.Send(new MessageSpecificationApiModel(), new MemoryStream())
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
        public async Task ThrowsExceptionIfStatusCodeIsNot200(HttpStatusCode statusCode)
        {
            var sut = _fixture.WithStatusCode(statusCode).CreateSut();

            await Assert.ThrowsAsync<FiksIoSendUnexpectedResponseException>(
                            async () => await sut.Send(new MessageSpecificationApiModel(), new MemoryStream())
                                                 .ConfigureAwait(false))
                        .ConfigureAwait(false);
        }

        [Fact]
        public async Task ThrowsUnauthorizedExceptionIfStatusCodeIs401()
        {
            var sut = _fixture.WithStatusCode(HttpStatusCode.Unauthorized).CreateSut();

            await Assert.ThrowsAsync<FiksIoSendUnauthorizedException>(
                            async () => await sut.Send(new MessageSpecificationApiModel(), new MemoryStream())
                                                 .ConfigureAwait(false))
                        .ConfigureAwait(false);
        }
        */
    }
}