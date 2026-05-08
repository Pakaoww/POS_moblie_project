using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace POS_moblie_project.Models;

[Table("Products")]
public class Product
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique, NotNull]
    public string ProductCode { get; set; }

    [NotNull]
    public string Name { get; set; }

    // Foreign key to Categories table
    public int CategoryId { get; set; }

    [NotNull]
    public decimal Price { get; set; }

    public int Stock { get; set; } = 0;

    // Controls whether product appears on POS page
    public bool IsVisible { get; set; } = true;

    // Stores local file path to product image
    public string ImagePath { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Default constructor required by SQLite-net-pcl
    public Product()
    {

    }

    public Product(string productCode, string name, int categoryId, decimal price, int stock = 0)
    {
        ProductCode = productCode;
        Name = name;
        CategoryId = categoryId;
        Price = price;
        Stock = stock;
        IsVisible = true;
        ImagePath = string.Empty;
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }
}
