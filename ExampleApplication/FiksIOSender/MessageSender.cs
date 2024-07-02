namespace ExampleApplication.FiksIOSender;

public class MessageSender
{
    private readonly IFiksIOSender _fiksIoSender;
    private readonly AppSettings _appSettings;

    private static readonly ILogger Log = Serilog.Log.ForContext(MethodBase.GetCurrentMethod()?.DeclaringType);


    public MessageSender(IFiksIOSender fiksIoSender, AppSettings appSettings)
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

            var payload = new FilePayload("testfile.txt");
            var metaData = new MeldingSpesifikasjonApiModel(
                _appSettings.FiksIoAccountId,
                toAccountId,
                messageType,
                ttl: (long)TimeSpan.FromDays(2).TotalMilliseconds,
                headere: new());

            var sentMessage = await _fiksIoSender
                .SendWithEncryptedData(metaData, payload)
                .ConfigureAwait(false);
            
            Log.Information("MessageSender - message sendt with messageid: {MessageId}", sentMessage.MeldingId);
            return sentMessage.MeldingId;
        }
        catch (Exception e)
        {
            Log.Error("MessageSender - could not send message to account id {AccountId}. Error: {ErrorMessage}",
                toAccountId, e.Message);
            throw;
        }
    }
}