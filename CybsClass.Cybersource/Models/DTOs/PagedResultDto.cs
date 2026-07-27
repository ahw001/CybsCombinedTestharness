using CybsClass.Cybersource.Models.BaseData;
using System.Text.Json.Serialization;

namespace CybsClass.Cybersource.Models.DTOs;

public class PagedResultDto<T>
{
    [JsonPropertyName("items")]
    public List<T> Items { get; set; } = new();

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("pageNumber")]
    public int PageNumber { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }
}
