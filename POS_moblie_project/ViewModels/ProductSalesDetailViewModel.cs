using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Bibliography;
using POS_moblie_project.Models;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

[QueryProperty(nameof(ProductId), "id")]
public partial class ProductSalesDetailViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private int productId;

    [ObservableProperty]
    private string productName = string.Empty;

    [ObservableProperty]
    private string productCode = string.Empty;

    [ObservableProperty]
    private string imagePath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<TransactionItem> salesHistory = new();

    [ObservableProperty]
    private int totalSold;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private DateTime fromDate = DateTime.Now.AddMonths(-1).Date;

    [ObservableProperty]
    private DateTime toDate = DateTime.Now.Date.AddDays(1).AddSeconds(-1);

    public ProductSalesDetailViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    partial void OnProductIdChanged(int value)
    {
        if (value > 0)
            _ = LoadAsync(value);
    }

    private async Task LoadAsync(int id)
    {
        IsLoading = true;
        try
        {
            var product = await _databaseService.GetProductAsync(id);
            if (product == null) return;

            ProductName = product.Name;
            ProductCode = product.ProductCode;
            ImagePath = product.ImagePath ?? string.Empty;

            var items = await _databaseService
                .GetTransactionItemsByProductAsync(id, FromDate, ToDate);

            SalesHistory.Clear();
            foreach (var item in items)
                SalesHistory.Add(item);

            TotalSold = items.Sum(i => i.Quantity);
        }
        catch (Exception ex)
        {
            await AppAlert.ShowErrorAsync("Error", ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}