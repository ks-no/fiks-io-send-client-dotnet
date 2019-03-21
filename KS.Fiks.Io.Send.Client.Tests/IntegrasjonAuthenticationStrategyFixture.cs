using System;
using Ks.Fiks.Maskinporten.Client;
using Moq;

namespace KS.Fiks.IO.Send.Client.Tests
{
    public class IntegrasjonAuthenticationStrategyFixture
    {
        private Guid _integrasjonId;
        private string _integrasjonPassword;

        public IntegrasjonAuthenticationStrategyFixture()
        {
            SetDefaultValues();
        }

        public Mock<IMaskinportenClient> MaskinportenClientMock { get; private set; }

        public IntegrasjonAuthenticationStrategy CreateSut()
        {
            SetupMocks();
            return new IntegrasjonAuthenticationStrategy(
                MaskinportenClientMock.Object,
                _integrasjonId,
                _integrasjonPassword);
        }

        public IntegrasjonAuthenticationStrategyFixture WithIntegrasjonId(Guid value)
        {
            _integrasjonId = value;
            return this;
        }

        public IntegrasjonAuthenticationStrategyFixture WithIntegrasjonPassword(string value)
        {
            _integrasjonPassword = value;
            return this;
        }

        private void SetDefaultValues()
        {
            _integrasjonId = Guid.NewGuid();
            _integrasjonPassword = "PASSWORD";
        }

        private void SetupMocks()
        {
            var maskinportenTokenAsJsonString = @"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";

            MaskinportenClientMock = new Mock<IMaskinportenClient>();
            MaskinportenClientMock.Setup(x => x.GetAccessToken(It.IsAny<string>()))
                                  .ReturnsAsync(
                                      new MaskinportenToken(maskinportenTokenAsJsonString, 1000));
        }
    }
}