using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Models;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

public partial class StockViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private List<ProductWithStock> _allProductsWithStock = new();
    private List<ProductLot> _allLots = new();
    private List<Category> _allCategories = new();
    private List<InventoryLogItem> _allInventoryLogItems = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStockTab))]
    [NotifyPropertyChangedFor(nameof(IsInventoryTab))]
    private int selectedTabIndex = 0;

    public bool IsStockTab => SelectedTabIndex == 0;
    public bool IsInventoryTab => SelectedTabIndex == 1;

    [ObservableProperty]
    private ObservableCollection<ProductWithStock> products = new();

    [ObservableProperty]
    private ObservableCollection<Category> categoryFilters = new();

    [ObservableProperty]
    private Category? selectedCategory;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<InventoryLogItem> inventoryLogs = new();

    [ObservableProperty]
    private bool isLoading;

    public StockViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();

    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedCategoryChanged(Category? value) => ApplyFilters();

    [RelayCommand]
    public async Task LoadStockAsync()
    {
        IsLoading = true;
        try
        {
            var rawProducts = (await _databaseService.GetAllProductsAsync())
                .Where(p => !p.IsDeleted).ToList();
            _allCategories = await _databaseService.GetAllCategoriesAsync();
            _allLots = await _databaseService.GetAllLotsAsync();

            _allProductsWithStock = new List<ProductWithStock>();
            foreach (var p in rawProducts)
            {
                var lots = _allLots.Where(l => l.ProductId == p.Id).ToList();
                var latestLot = lots.OrderByDescending(l => l.ReceivedAt).FirstOrDefault();
                _allProductsWithStock.Add(new ProductWithStock
                {
                    Product = p,
                    TotalStock = lots.Sum(l => l.Remaining),
                    LotCount = lots.Count,
                    LatestCostPrice = latestLot?.CostPrice ?? 0
                });
            }

            _allInventoryLogItems = new List<InventoryLogItem>();
            foreach (var lot in _allLots)
            {
                var product = rawProducts.FirstOrDefault(p => p.Id == lot.ProductId);
                if (product == null) continue;
                _allInventoryLogItems.Add(new InventoryLogItem
                {
                    LotId = lot.LotId,
                    ProductName = product.Name,
                    ProductCode = product.ProductCode,
                    CategoryId = product.CategoryId,
                    CostPrice = lot.CostPrice,
                    Quantity = lot.Quantity,
                    Remaining = lot.Remaining,
                    ReceivedAt = lot.ReceivedAt,
                    IsActive = lot.IsActive
                });
            }

            CategoryFilters.Clear();
            CategoryFilters.Add(new Category("All", -1));
            foreach (var c in _allCategories)
                CategoryFilters.Add(c);

            ApplyFilters();
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilters()
    {
        // Filter products (Stock tab)
        var filteredProducts = _allProductsWithStock.AsEnumerable();

        // Filter inventory logs
        var filteredLogs = _allInventoryLogItems.AsEnumerable();

        if (SelectedCategory != null && SelectedCategory.SortOrder != -1)
        {
            filteredProducts = filteredProducts.Where(p => p.CategoryId == SelectedCategory.Id);
            filteredLogs = filteredLogs.Where(l => l.CategoryId == SelectedCategory.Id);
        }

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.Trim().ToLowerInvariant();
            filteredProducts = filteredProducts.Where(p =>
                p.Name.ToLowerInvariant().Contains(search) ||
                p.ProductCode.ToLowerInvariant().Contains(search));
            filteredLogs = filteredLogs.Where(l =>
                l.ProductName.ToLowerInvariant().Contains(search) ||
                l.ProductCode.ToLowerInvariant().Contains(search));
        }

        Products.Clear();
        foreach (var p in filteredProducts)
            Products.Add(p);

        InventoryLogs.Clear();
        foreach (var log in filteredLogs.OrderByDescending(l => l.ReceivedAt))
            InventoryLogs.Add(log);
    }

    [RelayCommand]
    private async Task AddNewProductAsync()
        => await Shell.Current.GoToAsync("ProductDetailPage");

    [RelayCommand]
    private async Task AddStockAsync()
    {
        var products = await _databaseService.GetAllProductsAsync();
        if (products.Count == 0)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "No Products",
                "There are no existing products, please add new products first.",
                "OK");
            return;
        }
        await Shell.Current.GoToAsync("AddStockPage");
    }

    [RelayCommand]
    private async Task EditProductAsync(ProductWithStock item)
    {
        if (item == null) return;
        await Shell.Current.GoToAsync($"ProductDetailPage?id={item.ProductId}");
    }

    [RelayCommand]
    private async Task ToggleVisibilityAsync(ProductWithStock item)
    {
        if (item == null) return;

        if (!item.IsVisible && item.TotalStock <= 0)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Out of Stock",
                "The product is out of stock, unable to be sold.",
                "OK");
            return;
        }

        try
        {
            await _databaseService.ToggleProductVisibilityAsync(item.ProductId);
            item.Product.IsVisible = !item.Product.IsVisible;

            var index = Products.IndexOf(item);
            if (index >= 0)
            {
                Products.RemoveAt(index);
                Products.Insert(index, item);
            }
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task DeleteProductAsync(ProductWithStock item)
    {
        if (item == null) return;

        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Delete Product",
            $"Delete \"{item.Name}\" and all its lots?",
            "Delete", "Cancel");
        if (!confirm) return;

        try
        {
            var lots = await _databaseService.GetLotsByProductAsync(item.ProductId);
            foreach (var lot in lots)
                await _databaseService.DeleteLotAsync(lot.Id);
            await _databaseService.DeleteProductAsync(item.ProductId);
            Products.Remove(item);
            _allProductsWithStock.Remove(item);
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private void SwitchToStockTab() => SelectedTabIndex = 0;

    [RelayCommand]
    private void SwitchToInventoryLogTab() => SelectedTabIndex = 1;
}
// InventoryLogItem ย้ายไป Models/InventoryLogItem.cs แล้ว