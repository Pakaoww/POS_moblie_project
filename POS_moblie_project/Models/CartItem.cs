namespace POS_moblie_project.Models;

/// <summary>
/// In-memory only — not stored in SQLite
/// </summary>
public class CartItem
{
    public int ProductId { get; set; }
    public int LotId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public decimal Subtotal => UnitPrice * Quantity;

    public CartItem(Product product, ProductLot lot, int quantity = 1)
    {
        ProductId = product.Id;
        LotId = lot.Id;
        ProductCode = product.ProductCode;
        ProductName = product.Name;
        UnitCost = lot.CostPrice;
        UnitPrice = product.SalePrice;
        Quantity = quantity;
        ImagePath = product.ImagePath ?? string.Empty;
    }
}