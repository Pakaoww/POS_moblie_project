using POS_moblie_project.ViewModels;
using POS_moblie_project.Views.Splash;

namespace POS_moblie_project;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new SplashPage());
    }
}