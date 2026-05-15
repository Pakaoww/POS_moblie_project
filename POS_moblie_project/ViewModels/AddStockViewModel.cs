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
    private List<Product> _allProducts = new();

    // ── Search & filter ──────────────────────────────────
    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Product> searchResults = new();

    [ObservableProperty]
    private ObservableCollection<Product> allProducts = new();

    [ObservableProperty]
    private ObservableCollection<Category> categoryFilters = new();

    [ObservableProperty]
    private Category? selectedCategory;

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

    [RelayCommand]
    public async Task InitializeAsync()
    {
        _allProducts = (await _databaseService.GetAllProductsAsync())
            .Where(p => !p.IsDeleted).ToList();

        var cats = await _databaseService.GetAllCategoriesAsync();
        CategoryFilters.Clear();
        CategoryFilters.Add(new Category("All", -1));
        foreach (var c in cats)
            CategoryFilters.Add(c);

        ApplyFilters();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();

    partial void OnSelectedCategoryChanged(Category? value) => ApplyFilters();

    private void ApplyFilters()
    {
        var filtered = _allProducts.AsEnumerable();

        if (SelectedCategory != null && SelectedCategory.SortOrder != -1)
            filtered = filtered.Where(p => p.CategoryId == SelectedCategory.Id);

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var s = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(p =>
                p.Name.ToLowerInvariant().Contains(s) ||
                p.ProductCode.ToLowerInvariant().Contains(s));
        }

        AllProducts.Clear();
        foreach (var p in filtered)
            AllProducts.Add(p);
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
