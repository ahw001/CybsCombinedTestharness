using System.Text.Json.Serialization;

namespace CybsClient.Services.DTOs;

public class CategoryDto
{
    [JsonPropertyName("categoryId")]
    public int CategoryId { get; set; }

    [JsonPropertyName("categoryName")]
    public string CategoryName { get; set; } = null!;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("picture")]
    public string? Picture { get; set; }

    [JsonPropertyName("Error")]
    public string? Error { get; set; }
}


