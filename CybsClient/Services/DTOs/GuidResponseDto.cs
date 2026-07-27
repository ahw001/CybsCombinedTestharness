using CybsClient.Model.Cybersource.BaseData;
using System.Text.Json.Serialization;


namespace CybsClient.Services.DTOs
{
    public class GuidResponseDto
    {
        [JsonPropertyName("guid")]
        public string? Guid { get; set; }
    }
}
