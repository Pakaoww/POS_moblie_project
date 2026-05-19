using POS_moblie_project.Services;
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
        if (_viewModel.CurrentState == CartState.Cart)
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
                await AppAlert.ShowErrorAsync("Error", "Failed to capture receipt.");
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
            await AppAlert.ShowErrorAsync("Error", ex.Message);
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
            await AppAlert.ShowErrorAsync("Error", "MediaStore insert failed.");
            return false;
        }

        using var output = context.ContentResolver.OpenOutputStream(uri)!;
        await imageStream.CopyToAsync(output);

        await AppAlert.ShowSuccessAsync("Saved ✓", $"Saved in Pictures/POS\n{fileName}");
        return true;
    }
#endif
}