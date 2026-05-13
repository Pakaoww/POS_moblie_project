using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using ZXing.Common;

namespace POS_moblie_project.Views.Shared;

public partial class BarcodeScannerPage : ContentPage
{
    private bool _hasScanned = false;
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

        SetStatus("🔒 กำลังขอสิทธิ์กล้อง...", "White");

        var status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            SetStatus("❌ ไม่ได้รับสิทธิ์กล้อง", "Red");
            await DisplayAlert("Permission Denied",
                "Camera permission is required to scan barcodes.", "OK");
            await Navigation.PopModalAsync();
            return;
        }

        SetStatus("📷 กล้องพร้อม — เล็งไปที่บาร์โค้ด", "White");
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
        SetStatus("🔍 กำลังสแกน...", "White");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        DeinitializeCamera();
    }

    private void DeinitializeCamera()
    {
        BarcodeReader.IsEnabled = false;
        BarcodeReader.IsDetecting = false;
        BarcodeReader.IsVisible = false;
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
            ResultLabel.Text = $"อ่านได้: {value}";
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

        SetStatus("✅ อ่านได้แล้ว — กรุณายืนยัน", "Green");
        SetResult(first.Value);

        MainThread.BeginInvokeOnMainThread(async () =>
        {
            var confirm = await DisplayAlert(
                "ยืนยันบาร์โค้ด",
                $"อ่านได้: {first.Value}\nใช้ค่านี้หรือไม่?",
                "ยืนยัน", "สแกนใหม่");

            if (confirm)
            {
                _onScanned?.Invoke(first.Value);
                await Navigation.PopModalAsync();
            }
            else
            {
                // สแกนใหม่
                _hasScanned = false;
                SetStatus("🔍 กำลังสแกน...", "White");
                SetResult("");
                await Task.Delay(1500);
                BarcodeReader.IsDetecting = true;
            }
        });
    }

    // ── Import from Gallery ──────────────────────────────────────────────────────
    private async void OnImportClicked(object sender, EventArgs e)
    {
        try
        {
            BarcodeReader.IsDetecting = false;
            SetStatus("🖼 กำลังเปิดรูปภาพ...", "White");

            var result = await MediaPicker.Default.PickPhotoAsync();
            if (result == null)
            {
                SetStatus("🔍 กำลังสแกน...", "White");
                BarcodeReader.IsDetecting = true;
                return;
            }

            SetStatus("🔎 กำลังอ่านบาร์โค้ดจากรูป...", "White");
            var scannedValue = await DecodeImageAsync(result);

            if (!string.IsNullOrWhiteSpace(scannedValue))
            {
                SetStatus("✅ อ่านได้แล้ว — กรุณายืนยัน", "Green");
                SetResult(scannedValue);

                var confirm = await DisplayAlert(
                    "ยืนยันบาร์โค้ด",
                    $"อ่านได้: {scannedValue}\nใช้ค่านี้หรือไม่?",
                    "ยืนยัน", "สแกนใหม่");

                if (confirm)
                {
                    _hasScanned = true;
                    _onScanned?.Invoke(scannedValue);
                    await Navigation.PopModalAsync();
                }
                else
                {
                    SetStatus("🔍 กำลังสแกน...", "White");
                    SetResult("");
                    await Task.Delay(1500);
                    BarcodeReader.IsDetecting = true;
                }
            }
            else
            {
                SetStatus("❌ ไม่พบบาร์โค้ดในรูปภาพ", "Red");
                await DisplayAlert("Not Found",
                    "No barcode found in the selected image.", "OK");
                SetStatus("🔍 กำลังสแกน...", "White");
                await Task.Delay(1500);
                BarcodeReader.IsDetecting = true;
            }
        }
        catch (Exception ex)
        {
            SetStatus($"❌ Error: {ex.Message}", "Red");
            await DisplayAlert("Error", $"Failed to read image: {ex.Message}", "OK");
            await Task.Delay(1500);
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