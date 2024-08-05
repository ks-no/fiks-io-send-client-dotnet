using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Send.Client.Models
{
    public class SendtMeldingApiModel
    {
        [JsonProperty("meldingId")]
        public Guid MeldingId { get; set; }

        [JsonProperty("meldingType")]
        public string MeldingType { get; set; }

        [JsonProperty("avsenderKontoId")]
        public Guid AvsenderKontoId { get; set; }

        [JsonProperty("mottakerKontoId")]
        public Guid MottakerKontoId { get; set; }

        [JsonProperty("ttl")]
        public long Ttl { get; set; }

        [JsonProperty("dokumentlagerId")]
        public Guid? DokumentlagerId { get; set; }

        [JsonProperty("svarPaMelding")]
        public Guid? SvarPaMelding { get; set; }

        [JsonProperty("headere")]
        public Dictionary<string, string> Headere { get; set; }

    }

}
