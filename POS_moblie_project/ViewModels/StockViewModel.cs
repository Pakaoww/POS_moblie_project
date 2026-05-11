using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Models;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

public partial class StockViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private List<Product> _allProducts = new();
    private List<Category> _allCategories = new();

    [ObservableProperty]
    private ObservableCollection<Product> products = new();

    [ObservableProperty]
    private ObservableCollection<Category> categoryFilters = new();

    [ObservableProperty]
    private Category selectedCategory;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public StockViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    partial void OnSearchTextChanged(string value) => ApplyFilters();
    partial void OnSelectedCategoryChanged(Category value) => ApplyFilters();

    [RelayCommand]
    public async Task LoadStockAsync()
    {
        IsLoading = true;
        try
        {
            _allProducts = await _databaseService.GetAllProductsAsync();
            _allCategories = await _databaseService.GetAllCategoriesAsync();

            CategoryFilters.Clear();
            CategoryFilters.Add(new Category("All", -1));
            foreach (var c in _allCategories)
                CategoryFilters.Add(c);

            ApplyFilters();
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", $"Failed to load stock: {ex.Message}", "OK");
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
            var search = SearchText.Trim().ToLowerInvariant();
            filtered = filtered.Where(p =>
                p.Name.ToLowerInvariant().Contains(search) ||
                p.ProductCode.ToLowerInvariant().Contains(search));
        }

        Products.Clear();
        foreach (var p in filtered)
            Products.Add(p);
    }

    [RelayCommand]
    private async Task AddProductAsync()
    {
        await Shell.Current.GoToAsync("ProductDetailPage");
    }

    [RelayCommand]
    private async Task EditProductAsync(Product product)
    {
        if (product == null) return;
        await Shell.Current.GoToAsync($"ProductDetailPage?id={product.Id}");
    }

    [RelayCommand]
    private async Task ToggleVisibilityAsync(Product product)
    {
        if (product == null) return;
        try
        {
            await _databaseService.ToggleProductVisibilityAsync(product.Id);
            product.IsVisible = !product.IsVisible;

            var index = Products.IndexOf(product);
            if (index >= 0)
            {
                Products.RemoveAt(index);
                Products.Insert(index, product);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", $"Failed to toggle: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task DeleteProductAsync(Product product)
    {
        if (product == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Delete Product",
            $"Permanently delete \"{product.Name}\"?",
            "Delete", "Cancel");

        if (!confirm) return;

        try
        {
            await _databaseService.DeleteProductAsync(product.Id);
            Products.Remove(product);
            _allProducts.Remove(product);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", $"Failed to delete: {ex.Message}", "OK");
        }
    }
}