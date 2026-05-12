using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using ZXing.Common;
// ไม่ต้อง using ZXing ตรงๆ — ใช้ fully qualified แทน

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
            Formats = BarcodeFormats.All,
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

    // ── Camera scan ──────────────────────────────────────
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

    // ── Import from Gallery ──────────────────────────────
    private async void OnImportClicked(object sender, EventArgs e)
    {
        try
        {
            BarcodeReader.IsDetecting = false;

            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result == null)
            {
                BarcodeReader.IsDetecting = true;
                return;
            }

            var scannedValue = await DecodeImageAsync(result);

            if (!string.IsNullOrWhiteSpace(scannedValue))
            {
                _hasScanned = true;
                _onScanned?.Invoke(scannedValue);
                await Navigation.PopModalAsync();
            }
            else
            {
                await DisplayAlert("Not Found",
                    "No barcode found in the selected image.", "OK");
                BarcodeReader.IsDetecting = true;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to read image: {ex.Message}", "OK");
            BarcodeReader.IsDetecting = true;
        }
    }

    private async Task<string?> DecodeImageAsync(FileResult file)
    {
        try
        {
            using var stream = await file.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            var reader = new ZXing.BarcodeReaderGeneric
            {
                AutoRotate = true,
                Options = new DecodingOptions
                {
                    TryHarder = true,
                    // ใช้ ZXing.BarcodeFormat แบบ fully qualified
                    PossibleFormats = new List<ZXing.BarcodeFormat>
                    {
                        ZXing.BarcodeFormat.QR_CODE,
                        ZXing.BarcodeFormat.CODE_128,
                        ZXing.BarcodeFormat.CODE_39,
                        ZXing.BarcodeFormat.EAN_13,
                        ZXing.BarcodeFormat.EAN_8,
                        ZXing.BarcodeFormat.UPC_A,
                        ZXing.BarcodeFormat.UPC_E,
                        ZXing.BarcodeFormat.DATA_MATRIX,
                        ZXing.BarcodeFormat.PDF_417,
                        ZXing.BarcodeFormat.AZTEC,
                        ZXing.BarcodeFormat.CODABAR,
                    }
                }
            };

            var luminanceSource = await Task.Run(() =>
                DecodeRawBitmap(imageBytes));

            if (luminanceSource == null) return null;

            var decodeResult = reader.Decode(luminanceSource);
            return decodeResult?.Text;
        }
        catch
        {
            return null;
        }
    }

    private ZXing.LuminanceSource? DecodeRawBitmap(byte[] imageBytes)
    {
        try
        {
#if ANDROID
            var bitmap = Android.Graphics.BitmapFactory.DecodeByteArray(
                imageBytes, 0, imageBytes.Length);
            if (bitmap == null) return null;

            var width = bitmap.Width;
            var height = bitmap.Height;
            var pixels = new int[width * height];
            bitmap.GetPixels(pixels, 0, width, 0, 0, width, height);
            bitmap.Recycle();

            var rgbBytes = pixels.SelectMany(p => new byte[]
            {
                (byte)((p >> 16) & 0xFF),
                (byte)((p >> 8) & 0xFF),
                (byte)(p & 0xFF)
            }).ToArray();

            return new ZXing.RGBLuminanceSource(
                rgbBytes, width, height,
                ZXing.RGBLuminanceSource.BitmapFormat.RGB24);
#else
            return null;
#endif
        }
        catch
        {
            return null;
        }
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