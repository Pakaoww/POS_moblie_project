using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Models;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

public partial class SalesReportViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private List<SalesReportItem> _allItems = new();

    [ObservableProperty]
    private ObservableCollection<SalesReportItem> reportItems = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private DateTime fromDate = DateTime.Now.Date;

    [ObservableProperty]
    private DateTime toDate = DateTime.Now.Date.AddDays(1).AddSeconds(-1);

    [ObservableProperty]
    private bool isLoading;

    // Stats
    [ObservableProperty]
    private int totalItemsSold;

    [ObservableProperty]
    private int totalProductTypes;

    [ObservableProperty]
    private decimal totalRevenue;

    public SalesReportViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    partial void OnSearchTextChanged(string value) => ApplySearch();

    [RelayCommand]
    public async Task LoadReportAsync()
    {
        IsLoading = true;
        try
        {
            _allItems = await _databaseService
                .GetSalesReportAsync(FromDate, ToDate);

            ApplySearch();
            CalculateStats();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplySearch()
    {
        var filtered = _allItems.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(i =>
                i.ProductName.ToLowerInvariant().Contains(s) ||
                i.ProductCode.ToLowerInvariant().Contains(s));
        }

        ReportItems.Clear();
        foreach (var i in filtered)
            ReportItems.Add(i);
    }

    private void CalculateStats()
    {
        TotalItemsSold = _allItems.Sum(i => i.TotalQuantity);
        TotalProductTypes = _allItems.Count;
        TotalRevenue = _allItems.Sum(i => i.TotalRevenue);
    }

    [RelayCommand]
    private async Task ViewProductDetailAsync(SalesReportItem item)
    {
        if (item == null) return;
        await Shell.Current.GoToAsync(
            $"ProductSalesDetailPage?id={item.ProductId}");
    }
}