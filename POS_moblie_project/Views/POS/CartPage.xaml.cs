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
            // ✅ Capture เฉพาะ ReceiptContainer (ไม่มี Buttons)
            var screenshot = await ReceiptContainer.CaptureAsync();
            if (screenshot == null)
            {
                await Shell.Current.DisplayAlert("Error", "Failed to capture receipt.", "OK");
                return false;
            }

            using var stream = await screenshot.OpenReadAsync();
            var fileName = $"Receipt_{_viewModel.ReceiptTransactionId}_{DateTime.Now:yyyyMMdd_HHmmss}.png";

#if ANDROID
            return await SaveToGalleryAndroid(stream, fileName);
#endif
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Receipt] {ex}");
            await Shell.Current.DisplayAlert("Error", ex.Message, "OK");
            return false;
        }
    }

#if ANDROID
    private async Task<bool> SaveToGalleryAndroid(Stream imageStream, string fileName)
    {
        var context = Android.App.Application.Context;

        var values = new Android.Content.ContentValues();
        values.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
        values.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/png");
        values.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath,
            Android.OS.Environment.DirectoryPictures + "/POS");

        var uri = context.ContentResolver!.Insert(
            Android.Provider.MediaStore.Images.Media.ExternalContentUri!, values);

        if (uri == null)
        {
            await Shell.Current.DisplayAlert("Error", "MediaStore insert failed.", "OK");
            return false;
        }

        using var output = context.ContentResolver.OpenOutputStream(uri)!;
        await imageStream.CopyToAsync(output);

        await Shell.Current.DisplayAlert("Saved ✓", $"บันทึกใน Pictures/POS\n{fileName}", "OK");
        return true;
    }
#endif
}