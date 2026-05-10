using Microsoft.Extensions.DependencyInjection;
using POS_moblie_project.Services;
using POS_moblie_project.ViewModels;
using POS_moblie_project.Views.Auth;

namespace POS_moblie_project;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Resolve PasswordPage via DI
        var passwordPage = ServiceHelper.GetService<PasswordPage>();

        // Wrap in NavigationPage so the page has a proper window context
        var window = new Window(passwordPage);

        // Initialize database in the background after the window appears.
        // Per the course's guidance: don't block startup with DB init.
        window.Created += async (_, _) =>
        {
            var db = ServiceHelper.GetService<DatabaseService>();
            await db.InitializeAsync();
        };

        return window;
    }
}