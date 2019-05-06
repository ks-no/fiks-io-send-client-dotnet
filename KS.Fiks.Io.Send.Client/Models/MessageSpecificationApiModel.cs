using System;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Send.Client
{
    public class MessageSpecificationApiModel
    {
        [JsonProperty("avsenderKontoId")]
        public Guid AvsenderKontoId { get; set; }

        [JsonProperty("mottakerKontoId")]
        public Guid MottakerKontoId { get; set; }

        [JsonProperty("svarPaMelding")]
        public Guid SvarPaMelding { get; set; }

        [JsonProperty("ttl")]
        public long Ttl { get; set; }
    }
}