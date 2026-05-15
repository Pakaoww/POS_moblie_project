using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Models;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

public partial class AddStockViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private Product? _selectedProduct;

    // ── Search state ─────────────────────────────────────
    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Product> searchResults = new();

    [ObservableProperty]
    private bool isSearching;

    [ObservableProperty]
    private bool hasSelectedProduct;

    // ── Selected product info (read-only display) ────────
    [ObservableProperty]
    private string productCode = string.Empty;

    [ObservableProperty]
    private string productName = string.Empty;

    [ObservableProperty]
    private string categoryName = string.Empty;

    [ObservableProperty]
    private decimal salePrice;

    [ObservableProperty]
    private string imagePath = string.Empty;

    // ── Lot input ────────────────────────────────────────
    [ObservableProperty]
    private decimal costPrice;

    [ObservableProperty]
    private int quantity;

    public AddStockViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            SearchResults.Clear();
            return;
        }
        _ = SearchProductsAsync(value);
    }

    private async Task SearchProductsAsync(string query)
    {
        IsSearching = true;
        try
        {
            var results = await _databaseService.SearchProductsAsync(query);
            SearchResults.Clear();
            foreach (var p in results)
                SearchResults.Add(p);
        }
        finally
        {
            IsSearching = false;
        }
    }

    [RelayCommand]
    private async Task SelectProductAsync(Product product)
    {
        if (product == null) return;

        _selectedProduct = product;
        ProductCode = product.ProductCode;
        ProductName = product.Name;
        SalePrice = product.SalePrice;
        ImagePath = product.ImagePath ?? string.Empty;

        // Get category name
        try
        {
            var cat = await _databaseService.GetCategoryAsync(product.CategoryId);
            CategoryName = cat?.Name ?? string.Empty;
        }
        catch
        {
            CategoryName = string.Empty;
        }

        // Pre-fill CostPrice from latest lot
        var latestLot = await _databaseService.GetLatestLotAsync(product.Id);
        CostPrice = latestLot?.CostPrice ?? 0;

        // Reset quantity
        Quantity = 0;

        // Clear search
        SearchText = string.Empty;
        SearchResults.Clear();

        HasSelectedProduct = true;
    }

    [RelayCommand]
    private async Task ScanBarcodeAsync()
    {
        var tcs = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var scannerPage = new Views.Shared.BarcodeScannerPage(result =>
            tcs.TrySetResult(result));

        scannerPage.Disappearing += (s, e) =>
            tcs.TrySetResult(string.Empty);

        await Application.Current!.MainPage!.Navigation.PushModalAsync(scannerPage);

        var scannedValue = await tcs.Task;
        if (!string.IsNullOrWhiteSpace(scannedValue))
        {
            // Auto-search with scanned barcode
            var product = await _databaseService.GetProductByCodeAsync(scannedValue);
            if (product != null)
                await SelectProductAsync(product);
            else
                SearchText = scannedValue;
        }
    }

    [RelayCommand]
    private void ClearProduct()
    {
        _selectedProduct = null;
        HasSelectedProduct = false;
        ProductCode = string.Empty;
        ProductName = string.Empty;
        CategoryName = string.Empty;
        SalePrice = 0;
        ImagePath = string.Empty;
        CostPrice = 0;
        Quantity = 0;
        SearchText = string.Empty;
        SearchResults.Clear();
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (_selectedProduct == null)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Validation", "Please select a product first.", "OK");
            return;
        }
        if (CostPrice < 0)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Validation", "Cost price must be 0 or greater.", "OK");
            return;
        }
        if (Quantity <= 0)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Validation", "Quantity must be greater than 0.", "OK");
            return;
        }

        try
        {
            var lotId = await _databaseService.GenerateLotIdAsync();
            var lot = new ProductLot(lotId, _selectedProduct.Id, CostPrice, Quantity);
            await _databaseService.CreateLotAsync(lot);

            // เปิด IsVisible ถ้าปิดอยู่
            if (!_selectedProduct.IsVisible)
            {
                _selectedProduct.IsVisible = true;
                await _databaseService.UpdateProductAsync(_selectedProduct);
            }

            await Application.Current!.MainPage!.DisplayAlert(
                "Success",
                $"Added {Quantity} units to \"{ProductName}\"\nLot: {lotId}",
                "OK");

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
        => await Shell.Current.GoToAsync("..");
}