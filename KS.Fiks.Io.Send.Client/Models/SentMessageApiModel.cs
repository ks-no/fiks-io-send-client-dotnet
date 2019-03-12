using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace KS.Fiks.Io.Send.Client
{
    public class SentMessageApiModel
    {
        [JsonProperty("meldingId")]
        public Guid? MeldingId { get; set; }

        [JsonProperty("meldingType")]
        public string MeldingType { get; set; }

        [JsonProperty("avsenderKontoId")]
        public Guid? AvsenderKontoId { get; set; }

        [JsonProperty("mottakerKontoId")]
        public Guid? MottakerKontoId { get; set; }

        [JsonProperty("ttl")]
        public long Ttl { get; set; }

        [JsonProperty("dokumentlagerId")]
        public Guid? DokumentlagerId { get; set; }

        [JsonProperty("svarPaMelding")]
        public Guid? SvarPaMelding { get; set; }
    }
}