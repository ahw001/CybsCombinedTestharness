using System.Text.Json.Serialization;

namespace CybsClient.Model.OutboundObjects
{
    public class SimpleJsonObject
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;

        [JsonPropertyName("resource")]
        public string Resource { get; set; } = null!;

        [JsonPropertyName("isBoarding")]
        public bool IsBoarding { get; set; } = false;

        [JsonPropertyName("requestEncryption")]
        public string RequestEncryption { get; set; } = "none";

        [JsonPropertyName("responseEncryption")]
        public string ResponseEncryption { get; set; } = "none";

        [JsonPropertyName("httpMethod")]
        public string? HttpMethod { get; set; }
    }
}
