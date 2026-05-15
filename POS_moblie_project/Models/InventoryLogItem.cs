namespace POS_moblie_project.Models;

/// <summary>
/// In-memory — for Inventory Log display in StockPage
/// </summary>
public class InventoryLogItem
{
    public string LotId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public decimal CostPrice { get; set; }
    public int Quantity { get; set; }
    public int Remaining { get; set; }
    public DateTime ReceivedAt { get; set; }
    public bool IsActive { get; set; }
}