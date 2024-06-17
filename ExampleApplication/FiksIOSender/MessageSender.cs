using System.Reflection;
using KS.Fiks.IO.Encryption.Models;
using KS.Fiks.IO.Send.Client;
using KS.Fiks.IO.Send.Client.Models;
using ILogger = Serilog.ILogger;

namespace ExampleApplication.FiksIOSender;

public class MessageSender
{
    private readonly IFiksIOSender _fiksIoSender;
    private readonly AppSettings.AppSettings _appSettings;

    private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);


    public MessageSender(IFiksIOSender fiksIoSender, AppSettings.AppSettings appSettings)
    {
        _fiksIoSender = fiksIoSender;
        _appSettings = appSettings;
    }

    public async Task<Guid> Send(string messageType, Guid toAccountId)
    {
        try
        {
            var klientMeldingId = Guid.NewGuid();
            Log.Information(
                "MessageSender - sending messagetype {MessageType} to account id: {AccountId} with klientMeldingId {KlientMeldingId}",
                messageType, toAccountId, klientMeldingId);

            // TODO: Korrekt måte å gjøre det på? Må dispose en eller annen plass i all fall
            using var fileStream = new FileStream("testfile.txt", FileMode.Open);
            var payload = new List<IPayload> { new StreamPayload(fileStream, "testfile.txt") };

            var sendtMessage = await _fiksIoSender
                .SendWithEncryptedData(
                    new MeldingSpesifikasjonApiModel(
                        _appSettings.FiksIOSenderConfig.FiksIoAccountId,
                        toAccountId,
                        messageType,
                        ttl: (long)TimeSpan.FromDays(2).TotalMilliseconds,
                        headere: new()),
                    payload)
                .ConfigureAwait(false);
            Log.Information("MessageSender - message sendt with messageid: {MessageId}", sendtMessage.MeldingId);
            return sendtMessage.MeldingId;
        }
        catch (Exception e)
        {
            Log.Error("MessageSender - could not send message to account id {AccountId}. Error: {ErrorMessage}",
                toAccountId, e.Message);
            throw;
        }
    }
}