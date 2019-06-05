using System;
using Newtonsoft.Json;

namespace KS.Fiks.IO.Send.Client.Models
{
    public class MeldingSpesifikasjonApiModel
    {
        public MeldingSpesifikasjonApiModel(
            Guid avsenderKontoId,
            Guid mottakerKontoId,
            string meldingType,
            long ttl,
            Guid? svarPaMelding = null)
        {
            AvsenderKontoId = avsenderKontoId;
            MottakerKontoId = mottakerKontoId;
            MeldingType = meldingType;
            Ttl = ttl;
            SvarPaMelding = svarPaMelding;
        }

        [JsonProperty("avsenderKontoId")]
        public Guid AvsenderKontoId { get; }

        [JsonProperty("mottakerKontoId")]
        public Guid MottakerKontoId { get; }

        [JsonProperty("svarPaMelding")]
        public Guid? SvarPaMelding { get; }

        [JsonProperty("meldingType")]
        public string MeldingType { get; }

        [JsonProperty("ttl")]
        public long Ttl { get; }
    }
}