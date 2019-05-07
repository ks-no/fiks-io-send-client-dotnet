using System;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Send.Client.Models
{
    public class SentMessageApiModel
    {
        [JsonProperty("meldingId")]
        public Guid MessageId { get; set; }

        [JsonProperty("meldingType")]
        public string MessageType { get; set; }

        [JsonProperty("avsenderKontoId")]
        public Guid SenderAccountId { get; set; }

        [JsonProperty("mottakerKontoId")]
        public Guid ReceiverAccountId { get; set; }

        [JsonProperty("ttl")]
        public long Ttl { get; set; }

        [JsonProperty("dokumentlagerId")]
        public Guid? DokumentlagerId { get; set; }

        [JsonProperty("svarPaMelding")]
        public Guid? RelatedMessageId { get; set; }
    }
}