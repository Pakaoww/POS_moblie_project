namespace POS_moblie_project.Models;

/// <summary>
/// In-memory only — aggregated per-product sales data for the report page
/// </summary>
public class SalesReportItem
{
    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}