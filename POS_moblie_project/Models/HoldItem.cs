using CommunityToolkit.Mvvm.ComponentModel;

namespace POS_moblie_project.Models;

public partial class HoldItem : ObservableObject
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public Product Product { get; set; } = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtotal))]
    private int quantity;

    public decimal Subtotal => UnitPrice * Quantity;

    public HoldItem(Product product, int quantity = 1)
    {
        Product = product;
        ProductId = product.Id;
        ProductName = product.Name;
        UnitPrice = product.SalePrice;      // ← Price → SalePrice
        ImagePath = product.ImagePath ?? string.Empty;
        Quantity = quantity;
    }
}