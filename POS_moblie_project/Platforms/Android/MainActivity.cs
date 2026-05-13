using Android.App;
using Android.Content.PM;

namespace POS_moblie_project;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges =
        ConfigChanges.ScreenSize |
        ConfigChanges.Orientation |
        ConfigChanges.UiMode |
        ConfigChanges.ScreenLayout |
        ConfigChanges.SmallestScreenSize |
        ConfigChanges.Density |
        ConfigChanges.KeyboardHidden |   // ← เพิ่ม
        ConfigChanges.Keyboard           // ← เพิ่ม
)]
public class MainActivity : MauiAppCompatActivity
{
}
