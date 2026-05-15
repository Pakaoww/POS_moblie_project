using SQLite;

namespace POS_moblie_project.Models;

[Table("TransactionItems")]
public class TransactionItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int TransactionId { get; set; }
    public int ProductId { get; set; }
    public int LotId { get; set; }

    [NotNull]
    public string ProductName { get; set; } = string.Empty;

    public decimal UnitCost { get; set; }

    [NotNull]
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    [NotNull]
    public decimal Subtotal { get; set; }

    public TransactionItem()
    {
    }

    public TransactionItem(int transactionId, int productId, int lotId,
                           string productName, decimal unitCost,
                           decimal unitPrice, int quantity)
    {
        TransactionId = transactionId;
        ProductId = productId;
        LotId = lotId;
        ProductName = productName;
        UnitCost = unitCost;
        UnitPrice = unitPrice;
        Quantity = quantity;
        Subtotal = unitPrice * quantity;
    }
}