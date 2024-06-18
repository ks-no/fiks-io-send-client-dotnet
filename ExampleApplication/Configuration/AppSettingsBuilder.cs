namespace ExampleApplication.Configuration;

public static class AppSettingsBuilder
{
    public static AppSettings? CreateAppSettings(IConfiguration configuration)
    {
        return configuration.GetSection("FiksIOSenderConfig").Get<AppSettings>();
    }
}