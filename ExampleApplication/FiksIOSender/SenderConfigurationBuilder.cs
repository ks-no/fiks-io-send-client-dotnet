namespace ExampleApplication.FiksIOSender;

public static class SenderConfigurationBuilder
{
    public static FiksIOSenderConfiguration CreateConfiguration(AppSettings appSettings)
    {
        return new FiksIOSenderConfigurationBuilder()
            .WithAsiceSigningConfiguration(appSettings.AsiceSigningPublicKey, appSettings.AsiceSigningPrivateKey)
            .WithFiksIntegrasjonConfiguration(appSettings.FiksIoIntegrationId, appSettings.FiksIoIntegrationPassword)
            .WithApiConfiguration(null, appSettings.ApiScheme, appSettings.ApiHost, appSettings.ApiPort)
            .Build();
    }
}