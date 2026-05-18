using POS_moblie_project.Services;
using ZXing.Common;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace POS_moblie_project.Views.Shared;

public partial class BarcodeScannerPage : ContentPage
{
    private bool _hasScanned = false;
    private bool _isAnimating = false;
    private bool _isLeavingPage = false;
    private readonly Action<string> _onScanned;

    public BarcodeScannerPage(Action<string> onScanned)
    {
        InitializeComponent();
        _onScanned = onScanned;
        BarcodeReader.BarcodesDetected += OnBarcodesDetected;
        StartScanLineAnimation();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _isLeavingPage = false;
        _hasScanned = false;

        // ✅ รอให้ Window attach เสร็จก่อน
        await Task.Delay(300);

        // ถ้าออกจากหน้าไปแล้วระหว่าง delay ให้หยุด
        if (_isLeavingPage) return;

        SetStatus("🔒 Requesting camera permission...", "White");

        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            SetStatus("❌ Camera permission denied", "Red");
            await AppAlert.ShowWarningAsync("Permission Denied",
                    "Camera permission is required to scan barcodes.");
            _isLeavingPage = true;
            await Navigation.PopModalAsync();
            return;
        }

        SetStatus("📷 Camera ready — Point at the barcode", "White");
        InitializeCamera();
    }

    private void InitializeCamera()
    {
        BarcodeReader.IsVisible = true;
        BarcodeReader.Options = new BarcodeReaderOptions
        {
            TryInverted = false,
            TryHarder = true,
            AutoRotate = true,
            Formats = BarcodeFormat.Code128
                | BarcodeFormat.Code39
                | BarcodeFormat.Code93
                | BarcodeFormat.Ean13
                | BarcodeFormat.Ean8
                | BarcodeFormat.UpcA
                | BarcodeFormat.UpcE
                | BarcodeFormat.Codabar
                | BarcodeFormat.Pdf417
                | BarcodeFormat.DataMatrix
                | BarcodeFormat.QrCode
                | BarcodeFormat.Aztec,
            Multiple = false
        };
        BarcodeReader.CameraLocation = CameraLocation.Rear;
        BarcodeReader.IsEnabled = true;
        BarcodeReader.IsDetecting = true;
        SetStatus("🔍 Scanning...", "White");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // ✅ ปิดกล้องเฉพาะตอนออกจากหน้าจริงๆ ไม่ใช่ตอน Dialog โผล่
        if (_isLeavingPage)
            DeinitializeCamera();
    }

    private void DeinitializeCamera()
    {
        BarcodeReader.IsEnabled = false;
        BarcodeReader.IsDetecting = false;
        BarcodeReader.IsVisible = false;
    }

    private void ResetForRescan()
    {
        _hasScanned = false;
        _isLeavingPage = false; // ✅ Reset flag ด้วย
        SetResult("");
        InitializeCamera();
        StartScanLineAnimation();
    }

    private void SetStatus(string message, string color = "White")
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = message;
            StatusLabel.TextColor = Color.FromArgb(color switch
            {
                "Red" => "#FF4444",
                "Green" => "#4ECDC4",
                _ => "#FFFFFF"
            });
        });
    }

    private void SetResult(string value)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                ResultLabel.Text = string.Empty;
            }
            else
            {
                ResultLabel.Text = $"Scanned successfully: {value}";
            }
        });
    }

    // ── Camera scan ──────────────────────────────────────────────────────────────
    private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_hasScanned) return;

        var first = e.Results?.FirstOrDefault();
        if (first == null) return;

        _hasScanned = true;
        BarcodeReader.IsDetecting = false;

        SetStatus("✅ Barcode detected successfully — Please confirm", "Green");
        SetResult(first.Value);

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var confirm = await AppAlert.ConfirmAsync(
                "Confirm barcode", $"Read successfully: {first.Value}\nUse this value?",
                "Confirm", "Scan again");

            if (confirm)
            {
                _isLeavingPage = true;
                _onScanned?.Invoke(first.Value);
                await Navigation.PopModalAsync();
            }
            else
            {
                ResetForRescan();
            }
        });
    }

    // ── Import from Gallery ──────────────────────────────────────────────────────
    private async void OnImportClicked(object sender, EventArgs e)
    {
        try
        {
            BarcodeReader.IsDetecting = false;
            SetStatus("🖼 Opening image...", "White");

            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result == null)
            {
                ResetForRescan();
                return;
            }

            SetStatus("🔎 Reading barcode from image...", "White");
            var scannedValue = await DecodeImageAsync(result);

            if (!string.IsNullOrWhiteSpace(scannedValue))
            {
                SetStatus("✅ Barcode read successfully — Please confirm", "Green");
                SetResult(scannedValue);

                var confirm = await AppAlert.ConfirmAsync(
                    "Confirm barcode", $"Read successfully: {scannedValue}\nUse this value?",
                    "Confirm", "Scan again");

                if (confirm)
                {
                    _isLeavingPage = true;
                    _hasScanned = true;
                    _onScanned?.Invoke(scannedValue);
                    await Navigation.PopModalAsync();
                }
                else
                {
                    ResetForRescan();
                }
            }
            else
            {
                SetStatus("❌ No barcode found in the image", "Red");
                await AppAlert.ShowWarningAsync("Not Found", "No barcode found in the selected image.");
                ResetForRescan();
            }
        }
        catch (Exception ex)
        {
            SetStatus($"❌ Error: {ex.Message}", "Red");
            await AppAlert.ShowErrorAsync("Error", $"Failed to read image: {ex.Message}");
            ResetForRescan();
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

            var luminanceSource = await Task.Run(() => DecodeRawBitmap(imageBytes));
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
        _isLeavingPage = true;
        await Navigation.PopModalAsync();
    }

    private void StartScanLineAnimation()
    {
        if (_isAnimating) return;
        _isAnimating = true;
        _ = AnimateScanLine();
    }

    private async Task AnimateScanLine()
    {
        while (!_hasScanned)
        {
            await ScanLine.TranslateTo(0, -100, 1000, Easing.SinInOut);
            await ScanLine.TranslateTo(0, 100, 1000, Easing.SinInOut);
        }
        _isAnimating = false;
    }
}