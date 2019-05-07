using System;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Send.Client.Models
{
    public class MessageSpecificationApiModel
    {
        public MessageSpecificationApiModel(
            Guid senderAccountId,
            Guid receiverAccountId,
            string messageType,
            long ttl,
            Guid? relatedMessageId = null)
        {
            SenderAccountId = senderAccountId;
            ReceiverAccountId = receiverAccountId;
            MessageType = messageType;
            Ttl = ttl;
            RelatedMessageId = relatedMessageId;
        }

        [JsonProperty("avsenderKontoId")]
        public Guid SenderAccountId { get; }

        [JsonProperty("mottakerKontoId")]
        public Guid ReceiverAccountId { get; }

        [JsonProperty("svarPaMelding")]
        public Guid? RelatedMessageId { get; }

        [JsonProperty("meldingType")]
        public string MessageType { get; }

        [JsonProperty("ttl")]
        public long Ttl { get; }
    }
}