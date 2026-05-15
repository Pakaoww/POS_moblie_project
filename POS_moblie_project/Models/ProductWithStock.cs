namespace POS_moblie_project.Models;

/// <summary>
/// In-memory — Product + computed stock จาก lots
/// </summary>
public class ProductWithStock
{
    public Product Product { get; set; } = null!;
    public int TotalStock { get; set; }
    public int LotCount { get; set; }
    public decimal LatestCostPrice { get; set; }

    public int ProductId => Product.Id;
    public string Name => Product.Name;
    public string ProductCode => Product.ProductCode;
    public string ImagePath => Product.ImagePath;
    public decimal SalePrice => Product.SalePrice;
    public bool IsVisible => Product.IsVisible;
    public int CategoryId => Product.CategoryId;
}