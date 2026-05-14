using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.POS;

public partial class CartPage : ContentPage
{
    private readonly CartViewModel _viewModel;

    public CartPage(CartViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Wire the print event from ViewModel → View
        _viewModel.PrintReceiptRequested += CaptureReceiptAsync;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }

    private async Task<bool> CaptureReceiptAsync()
    {
        try
        {
            // Request media permissions first
            var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert(
                    "Permission Required",
                    "Storage permission is needed to save the receipt to your gallery.",
                    "OK");
                return false;
            }

            // Capture the full page as a screenshot
            IScreenshotResult? screenshot = await Screenshot.CaptureAsync();
            if (screenshot == null)
            {
                await Shell.Current.DisplayAlert("Error", "Failed to capture screenshot.", "OK");
                return false;
            }

            using var stream = await screenshot.OpenReadAsync();

            // Build a filename with transaction ID and timestamp
            var fileName = $"Receipt_{_viewModel.ReceiptTransactionId}_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            // Save to Pictures/POS folder
            var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            var folderPath = Path.Combine(picturesPath, "POS");
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, fileName);

            using (var fileStream = File.OpenWrite(filePath))
                await stream.CopyToAsync(fileStream);

#if ANDROID
            // Notify the Android media scanner so the image appears in Gallery
            var context = Android.App.Application.Context;
            Android.Media.MediaScannerConnection.ScanFile(
                context,
                new[] { filePath },
                new[] { "image/png" },
                null);
#endif

            await Shell.Current.DisplayAlert(
                "Receipt Saved",
                $"Receipt has been saved to your gallery.\n({fileName})",
                "OK");

            return true;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Could not save receipt: {ex.Message}", "OK");
            return false;
        }
    }
}