using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Models;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

public partial class POSViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private List<Product> _allProducts = new();
    private List<Category> _allCategories = new();

    [ObservableProperty]
    private ObservableCollection<ProductWithQuantity> products = new();

    [ObservableProperty]
    private ObservableCollection<Category> categoryFilters = new();

    [ObservableProperty]
    private Category selectedCategory;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private ObservableCollection<CartItem> cartItems = new();

    [ObservableProperty]
    private int cartCount;

    [ObservableProperty]
    private int holdCount;

    public POSViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedCategoryChanged(Category value) => ApplyFilters();

    [RelayCommand]
    public async Task LoadProductsAsync()
    {
        IsLoading = true;
        try
        {
            _allProducts = await _databaseService.GetVisibleProductsAsync();
            _allCategories = await _databaseService.GetAllCategoriesAsync();

            CategoryFilters.Clear();
            CategoryFilters.Add(new Category("All", -1));
            foreach (var c in _allCategories)
                CategoryFilters.Add(c);

            ApplyFilters();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

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

        Products.Clear();
        foreach (var p in filtered)
            Products.Add(new ProductWithQuantity(p, 0));
    }

    // stock จริง - ที่อยู่ใน cart - ที่ hold ไว้ทุก session
    private int GetAvailableStock(ProductWithQuantity item)
    {
        var inCart = CartItems
            .FirstOrDefault(c => c.ProductId == item.ProductId)?.Quantity ?? 0;
        var holdVm = ServiceHelper.GetService<HoldViewModel>();
        var inHold = holdVm.TotalReserved(item.ProductId);
        return item.Stock - inCart - inHold;
    }

    [RelayCommand]
    private void IncreaseQuantity(ProductWithQuantity item)
    {
        if (item == null) return;

        // หัก staging quantity ด้วย
        var available = GetAvailableStock(item) - item.Quantity;
        if (available <= 0)
        {
            Application.Current.MainPage.DisplayAlert(
                "Stock Limit",
                $"No more stock available for \"{item.Name}\".",
                "OK");
            return;
        }
        item.Quantity++;
    }

    [RelayCommand]
    private void DecreaseQuantity(ProductWithQuantity item)
    {
        if (item == null || item.Quantity <= 0) return;
        item.Quantity--;
    }

    [RelayCommand]
    private void AddToCart(ProductWithQuantity item)
    {
        if (item == null || item.Quantity <= 0) return;

        var available = GetAvailableStock(item);
        var addQty = Math.Min(item.Quantity, available);
        if (addQty <= 0) return;

        var existing = CartItems.FirstOrDefault(c => c.ProductId == item.ProductId);
        if (existing != null)
            existing.Quantity += addQty;
        else
            CartItems.Add(new CartItem(item.Product, addQty));

        CartCount = CartItems.Sum(c => c.Quantity);
        item.Quantity = 0;
    }

    [RelayCommand]
    private async Task AddToHoldAsync()
    {
        // Hold จาก CartItems ที่มีอยู่ ไม่ใช่ staging
        if (CartItems.Count == 0)
        {
            await Application.Current.MainPage.DisplayAlert(
                "No Items", "Please add items to cart before holding.", "OK");
            return;
        }

        var holdItems = CartItems
            .Select(c => new HoldItem(
                _allProducts.First(p => p.Id == c.ProductId),
                c.Quantity))
            .ToList();

        var holdViewModel = ServiceHelper.GetService<HoldViewModel>();
        holdViewModel.AddSession(holdItems);

        // เคลียร์ Cart หลัง hold
        CartItems.Clear();
        CartCount = 0;

        await Shell.Current.GoToAsync("HoldPage");
    }

    [RelayCommand]
    private async Task GoToCartAsync()
    {
        if (CartItems.Count == 0)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Cart Empty", "Please add items to cart first.", "OK");
            return;
        }
        await Shell.Current.GoToAsync("CartPage");
    }

    [RelayCommand]
    private async Task GoToHoldAsync()
        => await Shell.Current.GoToAsync("HoldPage");

    [RelayCommand]
    private async Task ScanBarcodeAsync()
    {
        var tcs = new TaskCompletionSource<string>();
        var scannerPage = new Views.Shared.BarcodeScannerPage(result =>
        {
            tcs.SetResult(result);
        });
        await Application.Current.MainPage.Navigation.PushModalAsync(scannerPage);
        var scannedValue = await tcs.Task;
        if (!string.IsNullOrWhiteSpace(scannedValue))
            SearchText = scannedValue;
    }

    public void ClearCart()
    {
        CartItems.Clear();
        CartCount = 0;
        foreach (var p in Products)
            p.Quantity = 0;
    }
}

public partial class ProductWithQuantity : ObservableObject
{
    public Product Product { get; }
    public int ProductId => Product.Id;
    public string Name => Product.Name;
    public string ProductCode => Product.ProductCode;
    public decimal Price => Product.Price;
    public int Stock => Product.Stock;
    public string ImagePath => Product.ImagePath;

    [ObservableProperty]
    private int quantity;

    public ProductWithQuantity(Product product, int quantity = 0)
    {
        Product = product;
        Quantity = quantity;
    }
}