using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using ExampleApplication.AppSettings;
using ExampleApplication.FiksIOSender;
using KS.Fiks.IO.Send.Client;
using KS.Fiks.IO.Send.Client.Authentication;
using Ks.Fiks.Maskinporten.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

var configurationBuilder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .Build();

var loggerFactory = InitSerilogConfiguration();
var logger = Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);
var appSettings = AppSettingsBuilder.CreateAppSettings(configurationBuilder);
var configuration = SenderConfigurationBuilder.CreateConfiguration(appSettings);
// var configuration = new FiksIOSenderConfigurationBuilder().Build();

var maskinportenClient = new MaskinportenClient(new MaskinportenClientConfiguration(
    appSettings.FiksIOSenderConfig.MaskinPortenAudienceUrl,
    appSettings.FiksIOSenderConfig.MaskinPortenTokenUrl,
    appSettings.FiksIOSenderConfig.MaskinPortenIssuer,
    numberOfSecondsLeftBeforeExpire: 10,
    certificate: new X509Certificate2(
        appSettings.FiksIOSenderConfig.MaskinPortenCompanyCertificatePath,
        appSettings.FiksIOSenderConfig.MaskinPortenCompanyCertificatePassword)));


// TODO: Prøv begge konstruktørene
// var fiksIOSender = new FiksIOSender(
//     senderConfiguration,
//     maskinportenClient,
//     appSettings.FiksIoSenderConfig.FiksIoIntegrationId,
//     appSettings.FiksIoSenderConfig.FiksIoIntegrationPassword,
//     httpClient);

var fiksIOSender = new FiksIOSender(
    configuration,
    new IntegrasjonAuthenticationStrategy(
        maskinportenClient,
        appSettings.FiksIOSenderConfig.FiksIoIntegrationId,
        appSettings.FiksIOSenderConfig.FiksIoIntegrationPassword));

var messageSender = new MessageSender(fiksIOSender, appSettings);
var toAccountId = appSettings.FiksIOSenderConfig.FiksIoAccountId;

var consoleKeyTask = Task.Run(() => { MonitorKeypress(); });
await new HostBuilder().RunConsoleAsync();
await consoleKeyTask;

async Task MonitorKeypress()
{
    logger.Information("Press Enter-key for sending a Fiks-IO 'ping' message");

    ConsoleKeyInfo cki;
    do 
    {
        //var cki = new ConsoleKeyInfo();
        // true hides the pressed character from the console
        cki = Console.ReadKey(true);

        var key = cki.Key;

        if (key == ConsoleKey.Enter)
        {
            logger.Information("Enter pressed. Sending FiksIoSender ping-message to account id: {ToAccountId}", toAccountId);
            await messageSender.Send("ping", toAccountId);
        }
    
        // Wait for an ESC
    } while (cki.Key != ConsoleKey.Escape);
}

static ILoggerFactory InitSerilogConfiguration()
{
    var loggerConfiguration = new LoggerConfiguration()
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.AspNetCore.Localization", LogEventLevel.Error)
        .Enrich.FromLogContext()
        .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level}] [{RequestId}] [{requestid}] - {Message} {NewLine} {Exception}");

    var logger = loggerConfiguration.CreateLogger();
    Log.Logger = logger;

    return LoggerFactory.Create(logging => logging.AddSerilog(logger));
}