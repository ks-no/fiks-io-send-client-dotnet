using Microsoft.Extensions.Configuration;

namespace ExampleApplication.Configuration;

public static class AppSettingsBuilder
{
    public static AppSettings? CreateAppSettings(IConfiguration configuration)
    {
        return configuration.GetSection("AppSettings").Get<AppSettings>();
    }
}