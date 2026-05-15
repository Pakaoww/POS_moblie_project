using SQLite;

namespace POS_moblie_project.Models;

[Table("Products")]
public class Product
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique, NotNull]
    public string ProductCode { get; set; } = string.Empty;

    [NotNull]
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    [NotNull]
    public decimal SalePrice { get; set; }

    public bool IsVisible { get; set; } = true;

    public bool IsDeleted { get; set; } = false;

    public string ImagePath { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public Product()
    {
    }

    public Product(string productCode, string name, int categoryId, decimal salePrice)
    {
        ProductCode = productCode;
        Name = name;
        CategoryId = categoryId;
        SalePrice = salePrice;
        IsVisible = true;
        ImagePath = string.Empty;
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }
}