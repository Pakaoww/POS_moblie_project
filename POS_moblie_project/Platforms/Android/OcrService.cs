using Android.Gms.Extensions;
using Xamarin.Google.MLKit.Vision.Common;
using Xamarin.Google.MLKit.Vision.Text;

namespace POS_moblie_project.Platforms.Android;

public class OcrService
{
    public async Task<string> RecognizeTextAsync(Stream imageStream)
    {
        var bitmap = await global::Android.Graphics.BitmapFactory.DecodeStreamAsync(imageStream);
        var image = InputImage.FromBitmap(bitmap, 0);

        var recognizer = TextRecognition.GetClient(
            Xamarin.Google.MLKit.Vision.Text.Latin.TextRecognizerOptions.DefaultOptions);

        var result = await recognizer.Process(image).AsAsync<Xamarin.Google.MLKit.Vision.Text.Text>();
        return result.GetText();
    }
}