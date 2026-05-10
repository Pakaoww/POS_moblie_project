using SQLite;

namespace POS_moblie_project.Models;

[Table("TransactionItems")]
public class TransactionItem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int TransactionId { get; set; }   // FK to Transactions.Id (the int PK)

    public int ProductId { get; set; }       // FK to Products.Id

    // Snapshot fields — historical receipts stay accurate even if product is later edited
    [NotNull]
    public string ProductName { get; set; }

    [NotNull]
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    [NotNull]
    public decimal Subtotal { get; set; }    // UnitPrice * Quantity

    public TransactionItem()
    {
    }

    public TransactionItem(int transactionId, int productId, string productName,
                           decimal unitPrice, int quantity)
    {
        TransactionId = transactionId;
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        Subtotal = unitPrice * quantity;
    }
}
