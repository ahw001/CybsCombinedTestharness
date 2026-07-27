namespace CybsClient.Model.CyberStore;

public class CartItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Variant { get; set; } = "";
    public string Sku { get; set; } = "";
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; } = 1;
    public string ImageUrl { get; set; } = "";

    public decimal LineTotal => UnitPrice * Quantity;
}
