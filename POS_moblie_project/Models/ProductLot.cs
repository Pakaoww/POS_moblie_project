using SQLite;

namespace POS_moblie_project.Models;

[Table("ProductLots")]
public class ProductLot
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique, NotNull]
    public string LotId { get; set; } = string.Empty;

    public int ProductId { get; set; }

    [NotNull]
    public decimal CostPrice { get; set; }

    public int Quantity { get; set; }
    public int Remaining { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.Now;

    public string Note { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ProductLot()
    {
    }

    public ProductLot(string lotId, int productId, decimal costPrice, int quantity)
    {
        LotId = lotId;
        ProductId = productId;
        CostPrice = costPrice;
        Quantity = quantity;
        Remaining = quantity;
        ReceivedAt = DateTime.Now;
        IsActive = true;
    }
}