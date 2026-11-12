using KS.Fiks.IO.Send.Client.Configuration;
using Shouldly;
using Xunit;

namespace KS.Fiks.IO.Send.Client.Tests.Configuration;

public class KatalogConfigurationTests
{
    private const string DefaultKontoPath = "/fiks-io/konto/api/v1";

    [Fact]
    public void KontoPathDefaultsToKontoApi()
    {
        new KatalogConfiguration().KontoPath.ShouldBe(DefaultKontoPath);
    }

    [Fact]
    public void KontoPathDefaultsToKontoApiWhenCreatedFromApiConfiguration()
    {
        new KatalogConfiguration(ApiConfiguration.CreateTestConfiguration()).KontoPath.ShouldBe(DefaultKontoPath);
    }

    [Fact]
    public void KontoPathCanBeOverridden()
    {
        new KatalogConfiguration(null, null, null, null, "/custom/konto/api/v1").KontoPath
            .ShouldBe("/custom/konto/api/v1");
    }
}
