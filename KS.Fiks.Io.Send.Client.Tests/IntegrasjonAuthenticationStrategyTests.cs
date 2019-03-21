using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;

namespace KS.Fiks.IO.Send.Client.Tests
{
    public class IntegrasjonAuthenticationStrategyTests
    {
        private IntegrasjonAuthenticationStrategyFixture _fixture;

        public IntegrasjonAuthenticationStrategyTests()
        {
            _fixture = new IntegrasjonAuthenticationStrategyFixture();
        }

        [Fact]
        public async Task GetsDictionaryWithAllExpectedKeys()
        {
            var sut = _fixture.CreateSut();

            var headers = await sut.GetAuthorizationHeaders().ConfigureAwait(false);

            headers.ContainsKey("AUTHORIZATION").Should().BeTrue();
            headers.ContainsKey("IntegrasjonId").Should().BeTrue();
            headers.ContainsKey("IntegrasjonPassord").Should().BeTrue();
        }

        [Fact]
        public async Task GetsExpectedIntegrasjonId()
        {
            var expectedId = Guid.NewGuid();
            var sut = _fixture.WithIntegrasjonId(expectedId).CreateSut();
            var headers = await sut.GetAuthorizationHeaders().ConfigureAwait(false);
            headers["IntegrasjonId"].Should().Be(expectedId.ToString());
        }

        [Fact]
        public async Task GetsExpectedIntegrasjonPassword()
        {
            var expectedPassword = "ExpectedPassword";
            var sut = _fixture.WithIntegrasjonPassword(expectedPassword).CreateSut();
            var headers = await sut.GetAuthorizationHeaders().ConfigureAwait(false);
            headers["IntegrasjonPassord"].Should().Be(expectedPassword);
        }

        [Fact]
        public async Task CallsMaskinportenGet()
        {
            var sut = _fixture.CreateSut();

            var header = await sut.GetAuthorizationHeaders().ConfigureAwait(false);

            _fixture.MaskinportenClientMock.Verify(x => x.GetAccessToken(It.IsAny<string>()));
        }
    }
}