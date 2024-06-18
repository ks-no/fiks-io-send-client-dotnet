using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using ExampleApplication.Configuration;
using ExampleApplication.FiksIOSender;
using KS.Fiks.IO.Send.Client;
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
var logger = Log.ForContext(MethodBase.GetCurrentMethod()!.DeclaringType!);
var appSettings = AppSettingsBuilder.CreateAppSettings(configurationBuilder);
var configuration = SenderConfigurationBuilder.CreateConfiguration(appSettings);
// var configuration = new FiksIOSenderConfigurationBuilder()
//     .WithAsiceSigningConfiguration(
//         appSettings.FiksIOSenderConfig.AsiceSigningPublicKey,
//         appSettings.FiksIOSenderConfig.AsiceSigningPrivateKey)
//     .WithFiksIntegrasjonConfiguration(
//         appSettings.FiksIOSenderConfig.FiksIoIntegrationId,
//         appSettings.FiksIOSenderConfig.FiksIoIntegrationPassword)
//     .WithApiConfiguration(null,
//         appSettings.FiksIOSenderConfig.ApiScheme,
//         appSettings.FiksIOSenderConfig.ApiHost,
//         appSettings.FiksIOSenderConfig.ApiPort)
//     .Build();

var maskinportenClient = new MaskinportenClient(new MaskinportenClientConfiguration(
    appSettings.FiksIOSenderConfig.MaskinPortenAudienceUrl,
    appSettings.FiksIOSenderConfig.MaskinPortenTokenUrl,
    appSettings.FiksIOSenderConfig.MaskinPortenIssuer,
    numberOfSecondsLeftBeforeExpire: 10,
    certificate: new X509Certificate2(
        appSettings.FiksIOSenderConfig.MaskinPortenCompanyCertificatePath,
        appSettings.FiksIOSenderConfig.MaskinPortenCompanyCertificatePassword)));

var fiksIoSender = new FiksIOSender(configuration, maskinportenClient);
var messageSender = new MessageSender(fiksIoSender, appSettings);
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