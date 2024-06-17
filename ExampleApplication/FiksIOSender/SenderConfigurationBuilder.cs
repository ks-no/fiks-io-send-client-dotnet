using KS.Fiks.IO.Send.Client.Configuration;

namespace ExampleApplication.FiksIOSender;

public static class SenderConfigurationBuilder
{
    public static FiksIOSenderConfiguration CreateConfiguration(AppSettings.AppSettings appSettings)
    {
        return new FiksIOSenderConfigurationBuilder()
            .WithAsiceSigningConfiguration(
                appSettings.FiksIOSenderConfig.AsiceSigningPublicKey,
                appSettings.FiksIOSenderConfig.AsiceSigningPrivateKey)
            .WithFiksIntegrasjonConfiguration(
                appSettings.FiksIOSenderConfig.FiksIoIntegrationId,
                appSettings.FiksIOSenderConfig.FiksIoIntegrationPassword)
            .WithApiConfiguration(null, appSettings.FiksIOSenderConfig.ApiScheme, appSettings.FiksIOSenderConfig.ApiHost, appSettings.FiksIOSenderConfig.ApiPort)
            .Build();
    }
}