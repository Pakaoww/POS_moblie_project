using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Models;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;

[QueryProperty(nameof(ProductId), "id")]
public partial class ProductDetailViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private Product _editingProduct;

    [ObservableProperty]
    private int productId;

    [ObservableProperty]
    private string productCode = string.Empty;

    [ObservableProperty]
    private string productName = string.Empty;

    [ObservableProperty]
    private decimal price;

    [ObservableProperty]
    private int stock;

    [ObservableProperty]
    private string imagePath = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private Category selectedCategory;

    [ObservableProperty]
    private string pageTitle = "New Product";

    [ObservableProperty]
    private bool isEditMode;

    public ProductDetailViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    partial void OnProductIdChanged(int value)
    {
        // Only trigger load when a real product ID is passed via query parameter
        if (value > 0)
            _ = LoadProductAsync(value);
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        // Always reload categories fresh each time the page appears
        var cats = await _databaseService.GetAllCategoriesAsync();
        Categories.Clear();
        foreach (var c in cats)
            Categories.Add(c);

        // Only set up Add mode defaults if we are not in edit mode
        if (ProductId == 0)
        {
            IsEditMode = false;
            PageTitle = "New Product";
            ProductCode = string.Empty;
            ProductName = string.Empty;
            Price = 0;
            Stock = 0;
            ImagePath = string.Empty;
            SelectedCategory = Categories.FirstOrDefault();
        }
    }

    private async Task LoadProductAsync(int id)
    {
        // Safety guard — should never be called with 0 but defensive check
        if (id <= 0) return;

        try
        {
            // Ensure categories are loaded before populating SelectedCategory
            if (Categories.Count == 0)
                await InitializeAsync();

            _editingProduct = await _databaseService.GetProductAsync(id);
            if (_editingProduct == null) return;

            ProductCode = _editingProduct.ProductCode;
            ProductName = _editingProduct.Name;
            Price = _editingProduct.Price;
            Stock = _editingProduct.Stock;
            ImagePath = _editingProduct.ImagePath ?? string.Empty;
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == _editingProduct.CategoryId);
            IsEditMode = true;
            PageTitle = "Edit Product";
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", $"Failed to load product: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Not Supported", "Camera capture is not supported on this device.", "OK");
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo != null)
                await SavePhotoAsync(photo);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", $"Camera error: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo != null)
                await SavePhotoAsync(photo);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", $"Gallery error: {ex.Message}", "OK");
        }
    }

    private async Task SavePhotoAsync(FileResult photo)
    {
        var fileName = $"product_{Guid.NewGuid()}.jpg";
        var localPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        using var sourceStream = await photo.OpenReadAsync();
        using var destStream = File.Create(localPath);
        await sourceStream.CopyToAsync(destStream);

        ImagePath = localPath;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        // Validation
        if (string.IsNullOrWhiteSpace(ProductCode))
        {
            await Application.Current.MainPage.DisplayAlert(
                "Validation", "Product code is required.", "OK");
            return;
        }

        if (string.IsNullOrWhiteSpace(ProductName))
        {
            await Application.Current.MainPage.DisplayAlert(
                "Validation", "Product name is required.", "OK");
            return;
        }

        if (Price < 0)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Validation", "Price must be 0 or greater.", "OK");
            return;
        }

        if (SelectedCategory == null)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Validation", "Please select a category.", "OK");
            return;
        }

        // Check for duplicate product code, excluding self when editing
        var excludeId = IsEditMode ? ProductId : 0;
        if (await _databaseService.IsProductCodeExistsAsync(ProductCode.Trim(), excludeId))
        {
            await Application.Current.MainPage.DisplayAlert(
                "Validation", "Product code already exists. Please use a different code.", "OK");
            return;
        }

        try
        {
            if (IsEditMode)
            {
                _editingProduct.ProductCode = ProductCode.Trim();
                _editingProduct.Name = ProductName.Trim();
                _editingProduct.CategoryId = SelectedCategory.Id;
                _editingProduct.Price = Price;
                _editingProduct.Stock = Stock;
                _editingProduct.ImagePath = ImagePath ?? string.Empty;
                await _databaseService.UpdateProductAsync(_editingProduct);
            }
            else
            {
                var product = new Product(
                    ProductCode.Trim(),
                    ProductName.Trim(),
                    SelectedCategory.Id,
                    Price,
                    Stock)
                {
                    ImagePath = ImagePath ?? string.Empty
                };
                await _databaseService.CreateProductAsync(product);
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", $"Failed to save product: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (!IsEditMode) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Delete Product",
            $"Permanently delete \"{ProductName}\"? This cannot be undone.",
            "Delete", "Cancel");

        if (!confirm) return;

        try
        {
            await _databaseService.DeleteProductAsync(ProductId);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", $"Failed to delete product: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    private async Task ScanBarcodeAsync()
    {
        var tcs = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        var scannerPage = new Views.Shared.BarcodeScannerPage(result =>
        {
            tcs.TrySetResult(result);
        });

        await Application.Current.MainPage.Navigation.PushModalAsync(scannerPage);

        var scannedValue = await tcs.Task;

        if (!string.IsNullOrWhiteSpace(scannedValue))
            ProductCode = scannedValue;
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}