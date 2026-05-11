using System;
using System.Collections.Generic;
using System.Text;

namespace POS_moblie_project.Models;

/// <summary>
/// In-memory only — not stored in SQLite
/// </summary>
public class CartItem
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public decimal Subtotal => UnitPrice * Quantity;

    public CartItem(Product product, int quantity = 1)
    {
        ProductId = product.Id;
        ProductCode = product.ProductCode;
        ProductName = product.Name;
        UnitPrice = product.Price;
        Quantity = quantity;
        ImagePath = product.ImagePath ?? string.Empty;
    }
}
