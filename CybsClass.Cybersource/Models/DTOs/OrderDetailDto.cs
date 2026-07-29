using CybsClass.Cybersource.Models.BaseData;
using System.Text.Json.Serialization;
namespace CybsClass.Cybersource.Models.DTOs;


public partial class OrderDetailDto
{

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public decimal UnitPrice { get; set; }

    public short Quantity { get; set; }

    public float Discount { get; set; }

    [JsonPropertyName("error")]
    public ErrorObject? Error { get; set; }

}
