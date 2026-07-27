using System.Text.Json.Serialization;

namespace CybsClient.Services.DTOs
{
    public class CloudPosBearerToken
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }
    }
}
