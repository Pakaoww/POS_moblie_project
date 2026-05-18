using POS_moblie_project.Services;
using POS_moblie_project.ViewModels;
using POS_moblie_project.Views.Auth;

namespace POS_moblie_project.Views.Splash;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Initialize database off the UI thread
            var db = ServiceHelper.GetService<DatabaseService>();
            await db.InitializeAsync();

            // Brief pause so splash is visible
            await Task.Delay(800);

            // Navigate to PasswordPage
            var passwordPage = ServiceHelper.GetService<PasswordPage>();
            Application.Current.MainPage = passwordPage;
        }
        catch (Exception ex)
        {
            await AppAlert.ShowErrorAsync("Startup Error", $"Failed to initialize the app: {ex.Message}");
        }
    }
}