using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace POS_moblie_project.Views.Shared;

public partial class BarcodeScannerPage : ContentPage
{
    private bool _hasScanned = false;
    private readonly Action<string> _onScanned;

    public BarcodeScannerPage(Action<string> onScanned)
    {
        InitializeComponent();
        _onScanned = onScanned;

        BarcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode
        | BarcodeFormat.Code128
        | BarcodeFormat.Code39
        | BarcodeFormat.Ean13
        | BarcodeFormat.Ean8,   // ← 0.7.4 ใช้ BarcodeFormat ไม่ใช่ BarcodeFormats
            AutoRotate = true,
            Multiple = false
        };

        StartScanLineAnimation();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Permission Denied",
                "Camera permission is required to scan barcodes.", "OK");
            await Navigation.PopModalAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        BarcodeReader.IsDetecting = false;
    }

    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_hasScanned) return;
        _hasScanned = true;

        var barcode = e.Results?.FirstOrDefault();
        if (barcode == null)
        {
            _hasScanned = false;
            return;
        }

        var value = barcode.Value;

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            BarcodeReader.IsDetecting = false;
            _onScanned?.Invoke(value);
            await Navigation.PopModalAsync();
        });
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private void StartScanLineAnimation()
    {
        _ = AnimateScanLine();
    }

    private async Task AnimateScanLine()
    {
        while (!_hasScanned)
        {
            await ScanLine.TranslateTo(0, -100, 1000, Easing.SinInOut);
            await ScanLine.TranslateTo(0, 100, 1000, Easing.SinInOut);
        }
    }
}