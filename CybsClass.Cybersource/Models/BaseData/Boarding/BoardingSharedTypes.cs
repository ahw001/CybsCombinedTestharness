using System.Text.Json.Serialization;

#nullable enable

namespace CybsClass.Cybersource.Models.BaseData.Boarding;

// Shared by UnifiedCheckoutBase and VirtualTerminalBase's "paste base JSON to load" parsers.
public sealed class ConfigurationStatus
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
