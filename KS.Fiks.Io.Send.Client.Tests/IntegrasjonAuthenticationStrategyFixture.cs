using System;
using Ks.Fiks.Maskinporten.Client;
using Moq;

namespace KS.Fiks.Io.Send.Client.Tests
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
            var maskinportenTokenAsJsonString = @"{
            ""aud"": ""oidc_ks_test"",
            ""scope"": ""ks"",
            ""iss"": ""https://oidc-ver2.difi.no/idporten-oidc-provider/"",
            ""token_type"": ""Bearer"",
            ""exp"": 1550837855,
            ""iat"": 1550837825,
            ""client_orgno"": ""987654321"",
            ""jti"": ""ifFO_xAYGepbtUxZhUcESoNkewGG6v15sfCWGPm_MUI=""
            }";

            MaskinportenClientMock = new Mock<IMaskinportenClient>();
            MaskinportenClientMock.Setup(x => x.GetAccessToken(It.IsAny<string>()))
                                  .ReturnsAsync(
                                      MaskinportenToken.CreateFromJsonString(maskinportenTokenAsJsonString, 1000));
        }
    }
}