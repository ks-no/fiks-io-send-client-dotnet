using System;
using System.Collections.Generic;
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
            Dictionary<string, string> headere,
            Guid? klientMeldingId = default,
            Guid? svarPaMelding = null)
        {
            AvsenderKontoId = avsenderKontoId;
            MottakerKontoId = mottakerKontoId;
            KlientMeldingId = klientMeldingId;
            MeldingType = meldingType;
            Ttl = ttl;
            Headere = headere;
            SvarPaMelding = svarPaMelding;
        }

        [JsonProperty("avsenderKontoId")]
        public Guid AvsenderKontoId { get; }

        [JsonProperty("mottakerKontoId")]
        public Guid MottakerKontoId { get; }

        [JsonProperty("svarPaMelding")]
        public Guid? SvarPaMelding { get; }

        [JsonProperty("klientMeldingId")]
        public Guid? KlientMeldingId { get; }

        [JsonProperty("meldingType")]
        public string MeldingType { get; }

        [JsonProperty("ttl")]
        public long Ttl { get; }

        [JsonProperty("headere")]
        public Dictionary<string, string> Headere { get; }
    }
}