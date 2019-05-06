using System;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Send.Client.Models
{
    public class MessageSpecificationApiModel
    {
        [JsonProperty("avsenderKontoId")]
        public Guid SenderAccountId { get; set; }

        [JsonProperty("mottakerKontoId")]
        public Guid ReceiverAccountId { get; set; }

        [JsonProperty("svarPaMelding")]
        public Guid RelatedMessageId { get; set; }

        [JsonProperty("ttl")]
        public long Ttl { get; set; }
    }
}