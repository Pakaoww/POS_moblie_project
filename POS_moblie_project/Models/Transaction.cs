using SQLite;

namespace POS_moblie_project.Models;

[Table("Transactions")]
public class Transaction
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique, NotNull]
    public string TransactionId { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.Now;

    [NotNull]
    public decimal TotalAmount { get; set; }    // Subtotal before discount & VAT

    // Discount
    public string DiscountType { get; set; } = "none";   // "none" | "percent" | "amount"
    public decimal DiscountValue { get; set; } = 0;      // % หรือจำนวนที่กรอก
    public decimal DiscountAmount { get; set; } = 0;     // ยอด discount เป็นบาท

    public decimal VatRate { get; set; } = 0;
    public decimal VatAmount { get; set; } = 0;

    [NotNull]
    public decimal GrandTotal { get; set; }     // TotalAmount - Discount + VAT

    public decimal MoneyReceived { get; set; }
    public decimal Change { get; set; }

    public Transaction()
    {
    }

    public Transaction(string transactionId, decimal totalAmount,
                       string discountType, decimal discountValue, decimal discountAmount,
                       decimal vatRate, decimal vatAmount, decimal grandTotal,
                       decimal moneyReceived, decimal change)
    {
        TransactionId = transactionId;
        Timestamp = DateTime.Now;
        TotalAmount = totalAmount;
        DiscountType = discountType;
        DiscountValue = discountValue;
        DiscountAmount = discountAmount;
        VatRate = vatRate;
        VatAmount = vatAmount;
        GrandTotal = grandTotal;
        MoneyReceived = moneyReceived;
        Change = change;
    }
}