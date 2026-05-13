using CommunityToolkit.Mvvm.ComponentModel;
using POS_moblie_project.Services;
using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Models;

public partial class HoldSession : ObservableObject
{
    public string SessionId { get; } = Guid.NewGuid().ToString("N")[..8].ToUpper();

    public DateTime CreatedAt { get; } = DateTime.Now;

    public System.Collections.ObjectModel.ObservableCollection<HoldItem> Items { get; } = new();

    public string Label => $"#{SessionId}";
    public string TimeLabel => CreatedAt.ToString("HH:mm");
    public decimal Total => Items.Sum(x => x.Subtotal);
    public int ItemCount => Items.Sum(x => x.Quantity);
    public string Summary => $"{ItemCount} item(s) · {ServiceHelper.GetService<CurrencyService>().Format(Total)}";
}