using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Graphics;

namespace POS_moblie_project.Models;

public partial class ProfitReportEntry : ObservableObject
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public decimal IncomeAmount { get; set; }
    public decimal ExpenseAmount { get; set; }
    public string EntryType { get; set; } = string.Empty;

    [ObservableProperty]
    private bool isDeleted;

    public string DisplayDescription => IsDeleted ? $"{Description} (Deleted)" : Description;
    public decimal DisplayAmount => EntryType == "Income" ? IncomeAmount : ExpenseAmount;
    public Color DisplayAmountColor => EntryType == "Income" ? Color.FromArgb("#4ECDC4") : Color.FromArgb("#E74C3C");
    public Color DisplayTypeColor => EntryType == "Income" ? Color.FromArgb("#4ECDC4") : Color.FromArgb("#E74C3C");
    public double DisplayOpacity => IsDeleted ? 0.5 : 1.0;
}
