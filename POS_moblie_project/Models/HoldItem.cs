//using AndroidX.Camera.Video;
using CommunityToolkit.Mvvm.ComponentModel;

namespace POS_moblie_project.Models;

public partial class HoldItem : ObservableObject
{
    public Product Product { get; }

    public int ProductId => Product.Id;

    public string ProductName => Product.Name;

    public decimal UnitPrice => Product.Price;

    [ObservableProperty]
    private int quantity;

    public decimal Subtotal => UnitPrice * Quantity;

    public HoldItem(Product product, int quantity)
    {
        Product = product;
        Quantity = quantity;
    }
}