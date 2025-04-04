using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using KS.Fiks.IO.Send.Client.Exceptions;
using KS.Fiks.IO.Send.Client.Models;
using Moq;
using Moq.Protected;
using Org.BouncyCastle.X509;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.Send.Client.Tests.Catalog;

public class CatalogHandlerTests
{
    private readonly CatalogHandlerFixture _fixture = new();

    [Fact]
    public async Task GetsExpectedAccount()
    {
        var expectedAccount = new KatalogKonto
        {
            KontoId = Guid.NewGuid(),
            KontoNavn = "accountName",
            FiksOrgId = Guid.NewGuid(),
            FiksOrgNavn = "orgName",
            Status = new KontoSvarStatus
            {
                Melding = "No melding",
                GyldigAvsender = true,
                GyldigMottaker = false,
                AntallKonsumenter = 3
            }
        };
        var sut = _fixture.WithAccountResponse(expectedAccount).CreateSut();

        var result = await sut.Lookup(_fixture.DefaultLookupRequest).ConfigureAwait(false);

        result.FiksOrgId.ShouldBe(expectedAccount.FiksOrgId);
        result.FiksOrgNavn.ShouldBe(expectedAccount.FiksOrgNavn);
        result.KontoId.ShouldBe(expectedAccount.KontoId);
        result.KontoNavn.ShouldBe(expectedAccount.KontoNavn);
        result.IsGyldigAvsender.ShouldBe(expectedAccount.Status.GyldigAvsender);
        result.IsGyldigMottaker.ShouldBe(expectedAccount.Status.GyldigMottaker);
        result.AntallKonsumenter.ShouldBe(expectedAccount.Status.AntallKonsumenter);
    }

    [Fact]
    public async Task GetsExpectedStatus()
    {
        var expectedStatus = new KontoSvarStatus()
        {
            Melding = "No melding",
            GyldigAvsender = true,
            GyldigMottaker = false,
            AntallKonsumenter = 3
        };
        var sut = _fixture.WithStatusResponse(expectedStatus).CreateSut();

        var result = await sut.GetStatus(_fixture.DefaultKontoId).ConfigureAwait(false);

        result.IsGyldigAvsender.ShouldBe(expectedStatus.GyldigAvsender);
        result.IsGyldigMottaker.ShouldBe(expectedStatus.GyldigMottaker);
        result.AntallKonsumenter.ShouldBe(expectedStatus.AntallKonsumenter);
        result.Melding.ShouldBe(expectedStatus.Melding);
    }

    [Fact]
    public async Task CallsExpectedUri()
    {
        var host = "api.fiks.dev.ks.no";
        var port = 443;
        var scheme = "https";
        var path = "/svarinn2/katalog/api/v1";

        var sut = _fixture.WithHost(host).WithPort(port).WithScheme(scheme).WithPath(path).CreateSut();

        var result = await sut.Lookup(_fixture.DefaultLookupRequest).ConfigureAwait(false);

        _fixture.HttpMessageHandleMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri.Port == port &&
                req.RequestUri.Host == host &&
                req.RequestUri.Scheme == scheme &&
                req.RequestUri.AbsolutePath == path + "/lookup"),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SetsExpectedAuthorizationHeader()
    {
        var expectedToken = Guid.NewGuid().ToString();

        var sut = _fixture.WithAccessToken(expectedToken).CreateSut();

        var result = await sut.Lookup(_fixture.DefaultLookupRequest).ConfigureAwait(false);

        _fixture.HttpMessageHandleMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(
                req =>
                    req.Headers.Authorization.Parameter == expectedToken &&
                    req.Headers.Authorization.Scheme == "Bearer"),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task SetsExpectedIntegrasjonIdAndPassword()
    {
        var expectedId = Guid.NewGuid();
        var expectedPassword = "myIntegrasjonPassword";

        var sut = _fixture.WithIntegrasjonId(expectedId).WithIntegrasjonPassword(expectedPassword).CreateSut();

        var result = await sut.Lookup(_fixture.DefaultLookupRequest).ConfigureAwait(false);

        _fixture.HttpMessageHandleMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(
                req =>
                    req.Headers.GetValues("integrasjonPassord").FirstOrDefault() ==
                    expectedPassword &&
                    req.Headers.GetValues("integrasjonId").FirstOrDefault() ==
                    expectedId.ToString()),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task UsesExpectedQueryParams()
    {
        var request = new LookupRequest(
            "testIdentifier",
            "testMessageType",
            3);

        var sut = _fixture.CreateSut();

        var result = await sut.Lookup(request).ConfigureAwait(false);

        Func<HttpRequestMessage, string, string> queryFromReq = (req, field) =>
            HttpUtility.ParseQueryString(req.RequestUri.Query)[field];
        _fixture.HttpMessageHandleMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(
                (req) =>
                    queryFromReq(req, "identifikator") == request.Identifikator &&
                    queryFromReq(req, "meldingProtokoll") == request.Meldingsprotokoll &&
                    int.Parse(queryFromReq(req, "sikkerhetsniva"), CultureInfo.InvariantCulture) == request.Sikkerhetsniva),
            ItExpr.IsAny<CancellationToken>());
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.Redirect)]
    [InlineData(HttpStatusCode.Forbidden)]
    public async Task ThrowsUnexpectedResponseExceptionWhenResponseIsNot200(HttpStatusCode statusCode)
    {
        var sut = _fixture.WithStatusCode(statusCode).CreateSut();

        await Assert.ThrowsAsync<FiksIOSendUnexpectedResponseException>(
                async () => await sut.Lookup(_fixture.DefaultLookupRequest).ConfigureAwait(false))
            .ConfigureAwait(false);
    }

    [Fact]
    public async Task GetPublicKeyUsesGetCallWithExpectedAccount()
    {
        var host = "api.fiks.dev.ks.no";
        var port = 443;
        var scheme = "https";
        var path = "/svarinn2/katalog/api/v1";

        var sut = _fixture.WithHost(host).WithPort(port).WithScheme(scheme).WithPath(path)
            .WithPublicKeyResponse(_fixture.CreateDefaultPublicKey()).CreateSut();
        var account = Guid.NewGuid();
        var result = await sut.GetPublicKey(account).ConfigureAwait(false);

        _fixture.HttpMessageHandleMock.Protected().Verify(
            "SendAsync",
            Times.Exactly(1),
            ItExpr.Is<HttpRequestMessage>(req =>
                req.RequestUri.Port == port &&
                req.RequestUri.Host == host &&
                req.RequestUri.Scheme == scheme &&
                req.RequestUri.AbsolutePath ==
                path + "/kontoer/" + account.ToString() + "/offentligNokkel"),
            ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task GetPublicKeyReturnsX509Object()
    {
        var sut = _fixture.WithPublicKeyResponse(_fixture.CreateDefaultPublicKey()).CreateSut();
        var result = await sut.GetPublicKey(Guid.NewGuid()).ConfigureAwait(false);
        result.ShouldBeOfType<X509Certificate>();
    }

    [Fact]
    public async Task GetKontoReturnsExpectedAccount()
    {
        var expectedAccount = new KatalogKonto
        {
            KontoId = Guid.NewGuid(),
            KontoNavn = "accountName",
            FiksOrgId = Guid.NewGuid(),
            FiksOrgNavn = "orgName",
            Organisasjonsnummer = "123456789",
            Kommunenummer = "1234",
            Status = new KontoSvarStatus
            {
                Melding = "Melding",
                GyldigAvsender = true,
                GyldigMottaker = false,
                AntallKonsumenter = 9
            }
        };
        var sut = _fixture.WithAccountResponse(expectedAccount).CreateSut();

        var result = await sut.GetKonto(Guid.NewGuid()).ConfigureAwait(true);

        result.FiksOrgId.ShouldBe(expectedAccount.FiksOrgId);
        result.FiksOrgNavn.ShouldBe(expectedAccount.FiksOrgNavn);
        result.Organisasjonsnummer.ShouldBe(expectedAccount.Organisasjonsnummer);
        result.KontoId.ShouldBe(expectedAccount.KontoId);
        result.KontoNavn.ShouldBe(expectedAccount.KontoNavn);
        result.Kommunenummer.ShouldBe(expectedAccount.Kommunenummer);
        result.IsGyldigAvsender.ShouldBe(expectedAccount.Status.GyldigAvsender);
        result.IsGyldigMottaker.ShouldBe(expectedAccount.Status.GyldigMottaker);
        result.AntallKonsumenter.ShouldBe(expectedAccount.Status.AntallKonsumenter);
    }
}