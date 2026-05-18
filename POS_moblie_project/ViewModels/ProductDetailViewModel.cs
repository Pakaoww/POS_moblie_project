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
    private Product? _editingProduct;

    [ObservableProperty] private int productId;
    [ObservableProperty] private string productCode = string.Empty;
    [ObservableProperty] private string productName = string.Empty;
    [ObservableProperty] private decimal salePrice;
    [ObservableProperty] private string imagePath = string.Empty;
    [ObservableProperty] private ObservableCollection<Category> categories = new();
    [ObservableProperty] private Category? selectedCategory;
    [ObservableProperty] private string pageTitle = "New Product";
    [ObservableProperty] private bool isEditMode;

    // First lot fields — Add mode only
    [ObservableProperty] private decimal costPrice;
    [ObservableProperty] private int initialQuantity;

    public ProductDetailViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    partial void OnProductIdChanged(int value)
    {
        if (value > 0) _ = LoadProductAsync(value);
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        var cats = await _databaseService.GetAllCategoriesAsync();
        Categories.Clear();
        foreach (var c in cats) Categories.Add(c);

        if (ProductId == 0)
        {
            IsEditMode = false;
            PageTitle = "New Product";
            ProductCode = string.Empty;
            ProductName = string.Empty;
            SalePrice = 0;
            CostPrice = 0;
            InitialQuantity = 0;
            ImagePath = string.Empty;
            SelectedCategory = Categories.FirstOrDefault();
        }
    }

    private async Task LoadProductAsync(int id)
    {
        if (id <= 0) return;
        try
        {
            if (Categories.Count == 0) await InitializeAsync();

            _editingProduct = await _databaseService.GetProductAsync(id);
            if (_editingProduct == null) return;

            ProductCode = _editingProduct.ProductCode;
            ProductName = _editingProduct.Name;
            SalePrice = _editingProduct.SalePrice;
            ImagePath = _editingProduct.ImagePath ?? string.Empty;
            SelectedCategory = Categories.FirstOrDefault(
                c => c.Id == _editingProduct.CategoryId);
            IsEditMode = true;
            PageTitle = "Edit Product";
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task TakePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported) return;
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo != null) await SavePhotoAsync(photo);
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task PickPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            if (photo != null) await SavePhotoAsync(photo);
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", ex.Message, "OK");
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
            ProductCode = scannedValue;
    }

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    private async Task SaveAsync()
    {
        // ── Validation — บังคับแค่ชื่อสินค้าอย่างเดียว ──────
        if (string.IsNullOrWhiteSpace(ProductName))
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Validation", "Product name is required.", "OK");
            return;
        }

        if (SelectedCategory == null)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Validation", "Please select a category.", "OK");
            return;
        }

        if (!IsEditMode && InitialQuantity <= 0)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Validation", "Initial quantity must be greater than 0.", "OK");
            return;
        }

        // เช็ค duplicate เฉพาะเมื่อกรอก ProductCode
        if (!string.IsNullOrWhiteSpace(ProductCode))
        {
            var excludeId = IsEditMode ? ProductId : 0;
            if (await _databaseService.IsProductCodeExistsAsync(ProductCode.Trim(), excludeId))
            {
                await Application.Current!.MainPage!.DisplayAlert(
                    "Validation", "Product code already exists.", "OK");
                return;
            }
        }

        try
        {
            if (IsEditMode)
            {
                _editingProduct!.ProductCode = ProductCode.Trim();
                _editingProduct.Name = ProductName.Trim();
                _editingProduct.CategoryId = SelectedCategory.Id;
                _editingProduct.SalePrice = SalePrice;      // 0 ได้
                _editingProduct.ImagePath = ImagePath;
                await _databaseService.UpdateProductAsync(_editingProduct);
            }
            else
            {
                var product = new Product(
                    ProductCode.Trim(),  // string.Empty ได้
                    ProductName.Trim(),
                    SelectedCategory.Id,
                    SalePrice)           // 0 ได้
                {
                    ImagePath = ImagePath ?? string.Empty,
                    IsVisible = true
                };
                await _databaseService.CreateProductAsync(product);

                var lotId = await _databaseService.GenerateLotIdAsync();
                var lot = new ProductLot(lotId, product.Id, CostPrice, InitialQuantity);
                await _databaseService.CreateLotAsync(lot);
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Application.Current!.MainPage!.DisplayAlert(
                "Error", ex.Message, "OK");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (!IsEditMode) return;

        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Delete Product",
            $"Delete \"{ProductName}\" and all its lots?",
            "Delete", "Cancel");
        if (!confirm) return;

        try
        {
            var lots = await _databaseService.GetLotsByProductAsync(ProductId);
            foreach (var lot in lots)
                await _databaseService.DeleteLotAsync(lot.Id);
            await _databaseService.DeleteProductAsync(ProductId);
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

    [RelayCommand]
    private async Task ScanProductImageAsync()
    {
        try
        {
            var action = await Shell.Current.DisplayActionSheet(
                "AI Product Detection",
                "Cancel",
                null,
                "📷 Take Photo",
                "🖼️ Choose from Gallery");

            if (action == null || action == "Cancel") return;

            FileResult? result = null;

            if (action == "📷 Take Photo")
            {
                if (!MediaPicker.Default.IsCaptureSupported)
                {
                    await Shell.Current.DisplayAlert(
                        "Not Supported", "Camera is not available.", "OK");
                    return;
                }
                result = await MediaPicker.Default.CapturePhotoAsync();
            }
            else if (action == "🖼️ Choose from Gallery")
            {
                result = await MediaPicker.Default.PickPhotoAsync();
            }

            if (result == null) return;

            IsLoading = true;

            var aiService = ServiceHelper.GetService<ProductAiService>();
            ProductSuggestion suggestion;

#if ANDROID
            using var stream1 = await result.OpenReadAsync();
            var ocrService = new Platforms.Android.OcrService();
            var rawText = await ocrService.RecognizeTextAsync(stream1);

            if (!string.IsNullOrWhiteSpace(rawText))
                suggestion = await aiService.AnalyzeTextAsync(rawText);
            else
            {
                using var stream2 = await result.OpenReadAsync();
                suggestion = await aiService.AnalyzeImageAsync(stream2);
            }
#else
        using var stream = await result.OpenReadAsync();
        suggestion = await aiService.AnalyzeImageAsync(stream);
#endif

            // ── ใส่แค่ ProductName อย่างเดียว ────────────────
            if (!string.IsNullOrWhiteSpace(suggestion.ProductName))
                ProductName = suggestion.ProductName;

            await Shell.Current.DisplayAlert(
                "AI Detection",
                $"Product: {suggestion.ProductName}",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}