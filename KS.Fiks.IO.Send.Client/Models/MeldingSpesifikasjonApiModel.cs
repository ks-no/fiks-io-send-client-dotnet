using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            Guid? svarPaMelding = null)
        {
            AvsenderKontoId = avsenderKontoId;
            MottakerKontoId = mottakerKontoId;
            MeldingType = meldingType;
            Ttl = ttl;
            Headere = headere;
            SvarPaMelding = svarPaMelding;
        }

        [JsonProperty("avsenderKontoId")] public Guid AvsenderKontoId { get; }

        [JsonProperty("mottakerKontoId")] public Guid MottakerKontoId { get; }

        [JsonProperty("svarPaMelding")] public Guid? SvarPaMelding { get; }

        [JsonProperty("meldingType")] public string MeldingType { get; }

        [JsonProperty("ttl")] public long Ttl { get; }

        [JsonProperty("headere")] public Dictionary<string, string> Headere { get; }

        public void Validate()
        {
            if (!IsValid(out var errors))
            {
                throw new ValidationException(string.Join(";", errors));
            }
        }

        public bool IsValid(out List<string> validationErrors)
        {
            validationErrors = new List<string>();

            if (GuidIsNotSet(AvsenderKontoId))
            {
                validationErrors.Add("AvsenderKontoId cannot be empty.");
            }

            if (GuidIsNotSet(MottakerKontoId))
            {
                validationErrors.Add("MottakerKontoId cannot be empty.");
            }

            if (GuidIsNotSet(SvarPaMelding))
            {
                validationErrors.Add("SvarPaMelding cannot be empty.");
            }

            if (string.IsNullOrEmpty(MeldingType))
            {
                validationErrors.Add("Meldingstype cannot be empty.");
            }

            if (Ttl <= 0)
            {
                validationErrors.Add("Ttl must be greater than zero.");
            }

            return validationErrors.Count == 0;
        }

        private bool GuidIsNotSet(Guid? guid) => guid == null || guid == Guid.Empty;
    }
}