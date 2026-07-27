using CybsClient.Model.Cybersource.BaseData;
using System.Text.Json.Serialization;

namespace CybsClient.Model.Cybersource.PASetupResponse
{
    public class PASetupResponse
    {

        [JsonPropertyName("clientReferenceInformation")]
        public ClientReferenceInformation? ClientReferenceInformation { get; set; }

        [JsonPropertyName("consumerAuthenticationInformation")]
        public ConsumerAuthenticationInformation? ConsumerAuthenticationInformation { get; set; }

        [JsonPropertyName("errorInformation")]
        public ErrorInformation? ErrorInformation { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("submitTimeUtc")]
        public DateTime? SubmitTimeUtc { get; set; }
    }
}