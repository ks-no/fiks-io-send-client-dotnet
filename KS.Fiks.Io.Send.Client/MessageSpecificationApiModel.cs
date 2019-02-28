using System;
using Newtonsoft.Json;

namespace KS.Fiks.Io.Send.Client
{
    public class MessageSpecificationApiModel
    {
        [JsonProperty("avsenderKontoId")]
        public Guid AvsenderKontoId { get; set; }
        
        [JsonProperty("mottakerKontoId")]
        public Guid MottakerKontoId { get; set; }
        
        [JsonProperty("meldingType")]
        public string MeldingType { get; set; }
        
        [JsonProperty("svarPaMelding")]
        public Guid SvarPaMelding { get; set; }
        
        [JsonProperty("ttl")]
        public long Ttl { get; set; }
    }
}

