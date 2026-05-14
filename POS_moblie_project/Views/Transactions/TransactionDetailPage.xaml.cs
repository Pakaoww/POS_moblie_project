using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Views.Transactions;

public partial class TransactionDetailPage : ContentPage
{
    private readonly TransactionDetailViewModel _viewModel;

    public TransactionDetailPage(TransactionDetailViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        _viewModel.PrintReceiptRequested += CaptureReceiptAsync;
    }

    private async Task<bool> CaptureReceiptAsync()
    {
        try
        {
            var screenshot = await ReceiptContainer.CaptureAsync();
            if (screenshot == null)
            {
                await Shell.Current.DisplayAlert("Error", "Failed to capture receipt.", "OK");
                return false;
            }

            using var stream = await screenshot.OpenReadAsync();
            var fileName = $"ReceiptCopy_{_viewModel.TransactionId}_{DateTime.Now:yyyyMMdd_HHmmss}.png";

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