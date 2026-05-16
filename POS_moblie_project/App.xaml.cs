using POS_moblie_project.Services;
using POS_moblie_project.ViewModels;
using POS_moblie_project.Views.Splash;

namespace POS_moblie_project;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        Application.Current!.UserAppTheme = AppTheme.Light;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new SplashPage());
    }

    protected override async void OnStart()
    {
        base.OnStart();
        await ServiceHelper.GetService<CurrencyService>().LoadAsync();
    }
}