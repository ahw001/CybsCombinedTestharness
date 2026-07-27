using System.Net;
using System.Text.Json.Serialization;

namespace CybsClient.Model.Cybersource.BaseData
{
    public class CloudPosError
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("developerDescription")]
        public string? DeveloperDescription { get; set; }
    }
}

