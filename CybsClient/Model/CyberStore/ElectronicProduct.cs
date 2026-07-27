using System.Text.Json.Serialization;
using CybsClient.Model.Cybersource.BaseData;

namespace CybsClient.Model.CyberStore;

public class ElectronicProduct
{
    public int ElectronicProductId { get; set; }
    public string ProductName { get; set; } = "";
    public string? ProductLabel { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? ProductSku { get; set; }
    public string? Picture { get; set; }
    public int? ProductLineId { get; set; }
    public string? Brand { get; set; }

    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }
}
