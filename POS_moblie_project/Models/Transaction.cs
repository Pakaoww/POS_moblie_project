using SQLite;

namespace POS_moblie_project.Models;

[Table("Transactions")]
public class Transaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique, NotNull]
    public string TransactionId { get; set; }   // Display ID (e.g. "19546972624")

    public DateTime Timestamp { get; set; } = DateTime.Now;

    [NotNull]
    public decimal TotalAmount { get; set; }    // Subtotal before VAT

    public decimal VatRate { get; set; } = 0;   // 0, 7, etc.
    public decimal VatAmount { get; set; } = 0;

    [NotNull]
    public decimal GrandTotal { get; set; }     // TotalAmount + VatAmount

    public decimal MoneyReceived { get; set; }
    public decimal Change { get; set; }

    public Transaction()
    {
    }

    public Transaction(string transactionId, decimal totalAmount, decimal vatRate,
                       decimal vatAmount, decimal grandTotal,
                       decimal moneyReceived, decimal change)
    {
        TransactionId = transactionId;
        Timestamp = DateTime.Now;
        TotalAmount = totalAmount;
        VatRate = vatRate;
        VatAmount = vatAmount;
        GrandTotal = grandTotal;
        MoneyReceived = moneyReceived;
        Change = change;
    }
}